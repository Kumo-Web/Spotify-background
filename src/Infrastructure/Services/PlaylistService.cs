using Application.Interfaces;
using SpotifyAPI.Web;

namespace Infrastructure.Services;

public class PlaylistService : IPlaylistService
{
    private readonly ITokenService _tokenService;

    public PlaylistService(ITokenService tokenService)
    {
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
       var token = await _tokenService.GetValidAccessTokenAsync(userId);
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        var config = SpotifyClientConfig
            .CreateDefault(token.AccessToken); 

        var spotifyClient = new SpotifyClient(config);
        var user = await spotifyClient.UserProfile.Current();
        return user;

    }

    public Task<List<string>> GetTopArtistsAsync(Guid userId, int limit)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetTopTracksAsync(Guid userId, int limit)
    {
        throw new NotImplementedException();
    }
}
