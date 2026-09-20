namespace Application.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Returns a usable Spotify access token, refreshing transparently if it has expired or is
    /// about to. Callers get only the access token — refresh tokens never leave this layer.
    /// </summary>
    /// <exception cref="Application.Exceptions.SpotifyNotConnectedException">
    /// If the user has no stored token, or the stored refresh token is no longer accepted.
    /// </exception>
    Task<string> GetValidAccessTokenAsync(string spotifyUserId, CancellationToken ct = default);
}
