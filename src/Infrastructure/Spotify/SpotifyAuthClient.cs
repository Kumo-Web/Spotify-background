using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Application.Interfaces;
using Application.Models;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Spotify;

/// <summary>
/// Talks to Spotify's OAuth endpoints. Hand-rolled rather than via SpotifyAPI.Web.Auth because
/// we need precise control over PKCE and the single-use state.
/// </summary>
internal sealed class SpotifyAuthClient(
    IHttpClientFactory httpClientFactory,
    IOptions<SpotifySettings> settings,
    ILogger<SpotifyAuthClient> logger) : ISpotifyAuthClient
{
    internal const string HttpClientName = "spotify-accounts";
    private const string AuthorizeUrl = "https://accounts.spotify.com/authorize";
    private const string TokenPath = "/api/token";

    private readonly SpotifySettings _settings = settings.Value;

    /// <summary>Spotify's token endpoint response. Wire shape only — never persisted directly.</summary>
    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("scope")] string? Scope);

    public Uri BuildAuthorizationUrl(string state, string codeChallenge)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = _settings.RedirectUrl,
            ["scope"] = _settings.ScopeString,
            ["state"] = state,
        };

        if (_settings.UsePkce)
        {
            query["code_challenge_method"] = "S256";
            query["code_challenge"] = codeChallenge;
        }

        // Built by hand rather than with QueryHelpers: that lives in the ASP.NET Core shared
        // framework, and Infrastructure is a plain class library that shouldn't depend on it.
        var queryString = string.Join(
            '&',
            query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return new Uri($"{AuthorizeUrl}?{queryString}");
    }

    public Task<SpotifyTokenGrant> ExchangeCodeAsync(
        string code, string codeVerifier, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _settings.RedirectUrl,
        };

        if (_settings.UsePkce)
        {
            form["code_verifier"] = codeVerifier;
        }

        return PostTokenRequestAsync(form, "authorization code exchange", ct);
    }

    public Task<SpotifyTokenGrant> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
        };

        return PostTokenRequestAsync(form, "token refresh", ct);
    }

    private async Task<SpotifyTokenGrant> PostTokenRequestAsync(
        Dictionary<string, string> form, string operation, CancellationToken ct)
    {
        // Spotify's PKCE flow expects client_id in the body; the confidential flow expects it in
        // the Basic header. Sending it in the body is harmless in both, and required by one.
        form["client_id"] = _settings.ClientId;

        using var request = new HttpRequestMessage(HttpMethod.Post, TokenPath)
        {
            Content = new FormUrlEncodedContent(form),
        };

        if (_settings.UseClientSecret)
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            // Log the status and Spotify's error body, never the form — it carries the code,
            // refresh token and client secret.
            logger.LogError(
                "Spotify {Operation} failed with {StatusCode}. Response: {Body}",
                operation, (int)response.StatusCode, Truncate(body, 500));

            throw new SpotifyAuthException(operation, response.StatusCode, Truncate(body, 500));
        }

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
            ?? throw new SpotifyAuthException(operation, response.StatusCode, "Empty response body.");

        return new SpotifyTokenGrant(
            AccessToken: payload.AccessToken,
            // Deliberately passed through as null when absent rather than defaulted to empty.
            // Spotify usually omits this on refresh, and the caller must preserve what it has.
            RefreshToken: payload.RefreshToken,
            ExpiresInSeconds: payload.ExpiresIn,
            TokenType: payload.TokenType,
            Scope: payload.Scope ?? string.Empty);
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}

/// <summary>A Spotify OAuth endpoint returned a non-success status.</summary>
internal sealed class SpotifyAuthException(
    string operation, System.Net.HttpStatusCode statusCode, string responseBody)
    : Exception($"Spotify {operation} failed with HTTP {(int)statusCode}: {responseBody}")
{
    public System.Net.HttpStatusCode StatusCode { get; } = statusCode;
}
