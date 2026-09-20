namespace Application.Exceptions;

/// <summary>
/// The user has no usable Spotify connection — never linked, or the refresh token has been
/// revoked or expired. The caller should prompt a re-link rather than retry.
/// </summary>
public sealed class SpotifyNotConnectedException(string spotifyUserId, string reason)
    : Exception($"Spotify is not connected for user '{spotifyUserId}': {reason}")
{
    public string SpotifyUserId { get; } = spotifyUserId;
}
