using System.Security.Claims;
using API.Auth;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain;
using Domain.Configuration;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    ISpotifyAuthClient authClient,
    IOAuthChallengeFactory challengeFactory,
    IOAuthStateRepository stateRepository,
    ITokenRepository tokenRepository,
    IUserRepository userRepository,
    IOptions<SpotifySettings> spotifySettings,
    IOptions<FrontendSettings> frontendSettings,
    ILogger<AuthController> logger) : ControllerBase
{
    private readonly SpotifySettings _spotify = spotifySettings.Value;
    private readonly FrontendSettings _frontend = frontendSettings.Value;

    /// <summary>
    /// Starts login by redirecting to Spotify.
    /// <para>
    /// Takes no user id. The previous equivalent accepted one in the request body, which is how
    /// any caller could begin a flow on behalf of anyone.
    /// </para>
    /// </summary>
    [HttpGet("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromQuery] string? returnPath, CancellationToken ct)
    {
        var challenge = challengeFactory.Create();

        await stateRepository.SaveAsync(
            new OAuthState
            {
                State = challenge.State,
                CodeVerifier = challenge.CodeVerifier,
                // Only a relative path is ever stored, so this can't become an open redirect.
                ReturnPath = SanitiseReturnPath(returnPath),
            },
            ct);

        var url = authClient.BuildAuthorizationUrl(challenge.State, challenge.CodeChallenge);

        return Redirect(url.ToString());
    }

    /// <summary>
    /// Spotify redirects here with an authorisation code. Exchanges it, establishes the session,
    /// and sends the browser back to the app.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(error))
        {
            logger.LogWarning("Spotify denied the authorisation request: {Error}", error);
            return RedirectToFrontend("/login?error=denied");
        }

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return RedirectToFrontend("/login?error=invalid_callback");
        }

        // Single-use: consuming the state both proves this callback belongs to a flow we started
        // and prevents the same code being replayed.
        var stored = await stateRepository.ConsumeAsync(state, ct);
        if (stored is null)
        {
            logger.LogWarning("Rejected an OAuth callback whose state was unknown, expired or already used.");
            return RedirectToFrontend("/login?error=invalid_state");
        }

        var grant = await authClient.ExchangeCodeAsync(code, stored.CodeVerifier, ct);

        if (string.IsNullOrWhiteSpace(grant.RefreshToken))
        {
            // Without a refresh token the connection is single-use, which is not a usable state.
            logger.LogError("Spotify's code exchange returned no refresh token.");
            return RedirectToFrontend("/login?error=no_refresh_token");
        }

        // Identify the user from the token itself rather than from anything the caller supplied.
        var profile = await FetchProfileAsync(grant.AccessToken, ct);

        var existing = await userRepository.GetAsync(profile.Id, ct);
        await userRepository.SaveAsync(new SpotifyUser
        {
            SpotifyUserId = profile.Id,
            DisplayName = profile.DisplayName ?? profile.Id,
            // GET /me no longer returns Country, so market is configured, and only defaulted
            // on first login.
            Market = existing?.Market ?? _spotify.DefaultMarket,
            CreatedAt = existing?.CreatedAt ?? DateTimeOffset.UtcNow,
            LastLoginAt = DateTimeOffset.UtcNow,
        }, ct);

        await tokenRepository.SaveAsync(new SpotifyToken
        {
            SpotifyUserId = profile.Id,
            AccessToken = grant.AccessToken,
            RefreshToken = grant.RefreshToken,
            TokenType = grant.TokenType,
            Scope = grant.Scope,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(grant.ExpiresInSeconds),
            ReceivedAt = DateTimeOffset.UtcNow,
        }, ct);

        await SignInAsync(profile.Id, profile.DisplayName ?? profile.Id);

        logger.LogInformation("Established a session for Spotify user {SpotifyUserId}.", profile.Id);

        // The access token is deliberately NOT in this redirect. It previously travelled in the
        // query string, which put a live credential into browser history, Referer headers and
        // every proxy log on the path.
        return RedirectToFrontend(stored.ReturnPath ?? "/dashboard");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>Whether this browser has a session, and who it belongs to.</summary>
    [HttpGet("session")]
    [AllowAnonymous]
    public IActionResult Session()
    {
        var userId = User.FindFirst(SessionClaims.SpotifyUserId)?.Value;

        return userId is null
            ? Ok(new { authenticated = false })
            : Ok(new
            {
                authenticated = true,
                displayName = User.FindFirst(SessionClaims.DisplayName)?.Value,
            });
    }

    private async Task<PrivateUser> FetchProfileAsync(string accessToken, CancellationToken ct)
    {
        // A one-off client: at this point there is no session, so the usual factory — which
        // reads the authenticated principal — has no user to resolve.
        var client = new SpotifyClient(SpotifyClientConfig.CreateDefault(accessToken));
        return await client.UserProfile.Current(ct);
    }

    private Task SignInAsync(string spotifyUserId, string displayName)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(SessionClaims.SpotifyUserId, spotifyUserId),
                new Claim(SessionClaims.DisplayName, displayName),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    private RedirectResult RedirectToFrontend(string path)
        => Redirect($"{_frontend.BaseUrl.TrimEnd('/')}{path}");

    /// <summary>
    /// Keeps only a same-site relative path, so a crafted <c>returnPath</c> cannot redirect the
    /// browser to an attacker's host after login.
    /// </summary>
    private static string? SanitiseReturnPath(string? returnPath)
    {
        if (string.IsNullOrWhiteSpace(returnPath))
        {
            return null;
        }

        // Must be rooted and must not begin a scheme-relative URL ("//evil.example").
        return returnPath.StartsWith('/') && !returnPath.StartsWith("//", StringComparison.Ordinal)
            ? returnPath
            : null;
    }
}
