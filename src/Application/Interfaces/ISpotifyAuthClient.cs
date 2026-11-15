using Application.Models;

namespace Application.Interfaces;

/// <summary>
/// Spotify's OAuth endpoints. Authorization Code flow with PKCE.
/// </summary>
public interface ISpotifyAuthClient
{
    /// <summary>
    /// Builds the URL to send the browser to.
    /// </summary>
    /// <param name="state">
    /// Server-generated, single-use, high-entropy. Never a user identifier — that was the
    /// previous design and it left the flow with no CSRF protection.
    /// </param>
    /// <param name="codeChallenge">S256 challenge derived from the stored PKCE verifier.</param>
    Uri BuildAuthorizationUrl(string state, string codeChallenge);

    /// <summary>Exchanges an authorisation code for tokens, proving possession via PKCE.</summary>
    Task<SpotifyTokenGrant> ExchangeCodeAsync(string code, string codeVerifier, CancellationToken ct = default);

    Task<SpotifyTokenGrant> RefreshAsync(string refreshToken, CancellationToken ct = default);
}
