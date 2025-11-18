using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Interfaces;
using SpotifyAPI.Web;

namespace Infrastructure.Services;

public class PlaylistService : IPlaylistService
{
    private readonly ITokenService _tokenService;
    private readonly ISpotifyClientFactory _spotifyClientFactory;

    public PlaylistService(ITokenService tokenService, ISpotifyClientFactory spotifyClientFactory)
    {
        _spotifyClientFactory = spotifyClientFactory;
        _tokenService = tokenService;
    }

    public Task CreatePlaylistAsync(Guid userId, string playlistName, List<string> trackUris)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetMostPlayedTracksAsync(Guid userId, int limit)
    {
        throw new NotImplementedException();
    }

    public async Task<PrivateUser> GetSpotifyUserInfoAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);
        var user = await spotifyClient.UserProfile.Current();
        return user;

    }

    public async Task<Paging<FullArtist>> GetTopArtistsAsync(Guid userId, int limit)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);
        var artists = await spotifyClient.Personalization.GetTopArtists(
           new PersonalizationTopRequest
           {
               Limit = 20,
               TimeRangeParam = PersonalizationTopRequest.TimeRange.MediumTerm
           }
       );
        return artists;
    }

    public Task<List<string>> GetTopTracksAsync(Guid userId, int limit)
    {
        throw new NotImplementedException();
    }
}
