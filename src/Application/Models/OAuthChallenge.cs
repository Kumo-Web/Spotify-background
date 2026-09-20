namespace Application.Models;

/// <summary>
/// A freshly generated OAuth login challenge.
/// </summary>
/// <param name="State">Single-use CSRF value, stored server-side and sent to Spotify.</param>
/// <param name="CodeVerifier">PKCE secret. Stored server-side; never sent to the browser.</param>
/// <param name="CodeChallenge">S256 hash of the verifier. Safe to send to Spotify.</param>
public sealed record OAuthChallenge(string State, string CodeVerifier, string CodeChallenge);
