using Infrastructure.Interfaces;
using SpotifyAPI.Web;

namespace Infrastructure.Services;

public class SpotifyUserService : ISpotifyUserService
{
    public Task<PrivateUser> GetCurrentUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Paging<FullArtist>> GetFollowedArtistsAsync(Guid userId, int limit = 50)
    {
        throw new NotImplementedException();
    }

    public Task<CursorPaging<PlayHistoryItem>> GetRecentlyPlayedTracksAsync(Guid userId, int limit = 50)
    {
        throw new NotImplementedException();
    }

    public Task<Paging<SavedTrack>> GetSavedTracksAsync(Guid userId, int limit = 50, int offset = 0)
    {
        throw new NotImplementedException();
    }

    public Task<Paging<FullArtist>> GetTopArtistsAsync(Guid userId, int limit, string timeRange = "medium_term")
    {
        throw new NotImplementedException();
    }

    public Task<Paging<FullTrack>> GetTopTracksAsync(Guid userId, int limit, string timeRange = "medium_term")
    {
        throw new NotImplementedException();
    }
}
