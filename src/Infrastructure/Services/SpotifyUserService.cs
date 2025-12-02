using Infrastructure.Interfaces;
using SpotifyAPI.Web;

namespace Infrastructure.Services;

public class SpotifyUserService : ISpotifyUserService
{
    private readonly ISpotifyClientFactory _spotifyClientFactory;

    public SpotifyUserService(ISpotifyClientFactory spotifyClientFactory)
    {
        _spotifyClientFactory = spotifyClientFactory;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception> <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<PrivateUser> GetCurrentUserAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);
        var user = await spotifyClient.UserProfile.Current();
        return user;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception> <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public async Task<List<FullArtist>> GetFollowedArtistsAsync(Guid userId, int limit = 50)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var allArtists = new List<FullArtist>();

        var artistRequest = new FollowOfCurrentUserRequest(FollowOfCurrentUserRequest.Type.Artist)
        {
            Limit = limit
        };

        var artists = await spotifyClient.Follow.OfCurrentUser(artistRequest);
        allArtists.AddRange(artists.Artists.Items);

        while (!string.IsNullOrEmpty(artists.Artists.Next))
        {
            var request = new FollowOfCurrentUserRequest(FollowOfCurrentUserRequest.Type.Artist)
            {
                After = artistRequest.After
            };

            artists = await spotifyClient.Follow.OfCurrentUser(request);
            allArtists.AddRange(artists.Artists.Items);
        }

        return allArtists;
    }

    public async Task<CursorPaging<PlayHistoryItem>> GetRecentlyPlayedTracksAsync(Guid userId, int limit = 50)
    {

        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var playHistory = await spotifyClient.Player.GetRecentlyPlayed(new PlayerRecentlyPlayedRequest
        {
            Limit = limit
        });

       return playHistory;
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
