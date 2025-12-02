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

    public Task AddTracksToPlaylistAsync(Guid userId, string playlistId, List<string> trackUris)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreatePlaylistAsync(Guid userId, string playlistName, string description = null, bool isPublic = false)
    {
        throw new NotImplementedException();
    }

    public Task DeletePlaylistAsync(Guid userId, string playlistId)
    {
        throw new NotImplementedException();
    }

    public Task<FullPlaylist> GetPlaylistAsync(Guid userId, string playlistId)
    {
        throw new NotImplementedException();
    }

    // public async Task<Paging<FullArtist>> GetTopArtistsAsync(Guid userId, int limit)
    // {
    //     if (userId == Guid.Empty)
    //         throw new ArgumentNullException(nameof(userId));

    //     var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);
    //     var artists = await spotifyClient.Personalization.GetTopArtists(
    //        new PersonalizationTopRequest
    //        {
    //            Limit = 20,
    //            TimeRangeParam = PersonalizationTopRequest.TimeRange.MediumTerm
    //        }
    //    );
    //     return artists;
    // }

    // public async Task<Paging<FullTrack>> GetTopTracksAsync(Guid userId, int limit)
    // {
    //     if (userId == Guid.Empty)
    //         throw new ArgumentNullException(nameof(userId));

    //     var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);
    //     var tracks = await spotifyClient.Personalization.GetTopTracks(
    //        new PersonalizationTopRequest
    //        {
    //            Limit = 20,
    //            TimeRangeParam = PersonalizationTopRequest.TimeRange.MediumTerm
    //        }
    //     );
        
    //     return tracks;
    // }

    public Task<List<FullPlaylist>> GetUserPlaylistsAsync(Guid userId, int limit = 50)
    {
        throw new NotImplementedException();
    }

    public Task RemoveTracksFromPlaylistAsync(Guid userId, string playlistId, List<string> trackUris)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePlaylistDetailsAsync(Guid userId, string playlistId, string name, string description)
    {
        throw new NotImplementedException();
    }
}
