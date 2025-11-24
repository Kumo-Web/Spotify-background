using SpotifyAPI.Web;

namespace Infrastructure.Interfaces;

public interface ISpotifyUserService
{
    Task<PrivateUser> GetCurrentUserAsync(Guid userId);
    Task<Paging<FullTrack>> GetTopTracksAsync(Guid userId, int limit, string timeRange = "medium_term");
    Task<Paging<FullArtist>> GetTopArtistsAsync(Guid userId, int limit, string timeRange = "medium_term");
    Task<Paging<SavedTrack>> GetSavedTracksAsync(Guid userId, int limit = 50, int offset = 0);
    Task<CursorPaging<PlayHistoryItem>> GetRecentlyPlayedTracksAsync(Guid userId, int limit = 50);
    Task<Paging<FullArtist>> GetFollowedArtistsAsync(Guid userId, int limit = 50);
}
