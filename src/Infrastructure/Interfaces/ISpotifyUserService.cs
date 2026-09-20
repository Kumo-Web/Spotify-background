using SpotifyAPI.Web;

namespace Infrastructure.Interfaces;

/// <summary>
/// Reads the current user's Spotify data. No method takes a user id — see
/// <see cref="ISpotifyClientFactory"/> for why.
/// </summary>
public interface ISpotifyUserService
{
    Task<PrivateUser> GetProfileAsync(CancellationToken ct = default);

    Task<IReadOnlyList<FullArtist>> GetTopArtistsAsync(
        PersonalizationTopRequest.TimeRange range, int limit = 50, CancellationToken ct = default);

    Task<IReadOnlyList<FullTrack>> GetTopTracksAsync(
        PersonalizationTopRequest.TimeRange range, int limit = 50, CancellationToken ct = default);

    Task<IReadOnlyList<PlayHistoryItem>> GetRecentlyPlayedAsync(
        int limit = 50, CancellationToken ct = default);

    Task<IReadOnlyList<SavedTrack>> GetSavedTracksAsync(
        int maxItems = 200, CancellationToken ct = default);

    Task<IReadOnlyList<FullArtist>> GetFollowedArtistsAsync(
        int maxItems = 200, CancellationToken ct = default);

    /// <summary>Null when nothing is playing, or playback is on a private session.</summary>
    Task<CurrentlyPlaying?> GetCurrentlyPlayingAsync(CancellationToken ct = default);
}
