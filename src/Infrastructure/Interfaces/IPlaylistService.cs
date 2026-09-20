using SpotifyAPI.Web;

namespace Infrastructure.Interfaces;

/// <summary>
/// Playlist operations for the current user.
/// <para>
/// Namespace note: this genuinely belongs in Infrastructure because it exposes SpotifyAPI.Web
/// types. The previous file sat in this folder but declared <c>namespace Application.Interfaces</c>,
/// which quietly leaked Spotify types into the Application layer's apparent surface.
/// </para>
/// </summary>
public interface IPlaylistService
{
    Task<IReadOnlyList<FullPlaylist>> GetMyPlaylistsAsync(
        int maxItems = 200, CancellationToken ct = default);

    Task<FullPlaylist> GetAsync(string playlistId, CancellationToken ct = default);

    /// <summary>Creates a playlist owned by the current user. Returns its id.</summary>
    Task<string> CreateAsync(
        string name, string? description = null, bool isPublic = false, CancellationToken ct = default);

    /// <summary>Appends tracks, batching to Spotify's 100-per-request limit.</summary>
    Task AddTracksAsync(
        string playlistId, IReadOnlyList<string> trackUris, CancellationToken ct = default);

    Task RemoveTracksAsync(
        string playlistId, IReadOnlyList<string> trackUris, CancellationToken ct = default);

    /// <summary>Replaces the entire contents. Used by the weekly job to refresh its playlist.</summary>
    Task ReplaceTracksAsync(
        string playlistId, IReadOnlyList<string> trackUris, CancellationToken ct = default);

    Task UpdateDetailsAsync(
        string playlistId, string? name = null, string? description = null, CancellationToken ct = default);

    /// <summary>
    /// Removes the playlist from the user's library.
    /// <para>
    /// Spotify has no "delete playlist" operation — a playlist is removed by unfollowing it,
    /// which is what the client's delete button actually does. Named honestly so callers aren't
    /// misled into thinking the data is destroyed.
    /// </para>
    /// </summary>
    Task UnfollowAsync(string playlistId, CancellationToken ct = default);
}
