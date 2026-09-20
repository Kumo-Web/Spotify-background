namespace Application.Models;

/// <summary>
/// The result of an OAuth token grant — the wire-level outcome, not the stored entity.
/// </summary>
/// <param name="RefreshToken">
/// Nullable on purpose. Spotify frequently omits <c>refresh_token</c> from refresh responses,
/// and the previous code copied the absent value over the stored one — blanking the refresh
/// token and disconnecting the account after its first refresh. Making it nullable here forces
/// every caller to decide what happens when it's missing.
/// </param>
public sealed record SpotifyTokenGrant(
    string AccessToken,
    string? RefreshToken,
    int ExpiresInSeconds,
    string TokenType,
    string Scope);
