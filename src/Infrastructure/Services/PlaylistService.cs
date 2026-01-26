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

    public async Task AddTracksToPlaylistAsync(Guid userId, string playlistId, List<string> trackUris)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(playlistId))
            throw new ArgumentNullException(nameof(playlistId));
        if (trackUris == null || trackUris.Count == 0)
            throw new ArgumentNullException(nameof(trackUris));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        // Spotify API limits to 100 tracks per request
        const int batchSize = 100;
        for (int i = 0; i < trackUris.Count; i += batchSize)
        {
            var batch = trackUris.Skip(i).Take(batchSize).ToList();
            var request = new PlaylistAddItemsRequest
            {
                Uris = batch
            };
            await spotifyClient.Playlists.AddItems(playlistId, request);
        }
    }

    public async Task<string> CreatePlaylistAsync(Guid userId, string playlistName, string description = null, bool isPublic = false)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(playlistName))
            throw new ArgumentNullException(nameof(playlistName));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        // Get the user's Spotify ID
        var currentUser = await spotifyClient.UserProfile.Current();

        var request = new PlaylistCreateRequest(playlistName)
        {
            Description = description,
            Public = isPublic
        };

        var playlist = await spotifyClient.Playlists.Create(currentUser.Id, request);

        return playlist.Id;
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

    public async Task<List<FullPlaylist>> GetUserPlaylistsAsync(Guid userId, int limit = 50)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var allPlaylists = new List<FullPlaylist>();

        var playlists = await spotifyClient.Playlists.CurrentUsers(
            new PlaylistCurrentUsersRequest
            {
                Limit = limit
            }
        );

        // Fetch full details for each playlist
        if (playlists.Items != null)
        {
            foreach (var simplePlaylist in playlists.Items)
            {
                var fullPlaylist = await spotifyClient.Playlists.Get(simplePlaylist.Id);
                allPlaylists.Add(fullPlaylist);
            }
        }

        // Handle pagination
        while (!string.IsNullOrEmpty(playlists.Next) && allPlaylists.Count < limit)
        {
            playlists = await spotifyClient.Playlists.CurrentUsers();
            if (playlists.Items != null)
            {
                foreach (var simplePlaylist in playlists.Items)
                {
                    if (allPlaylists.Count >= limit) break;
                    var fullPlaylist = await spotifyClient.Playlists.Get(simplePlaylist.Id);
                    allPlaylists.Add(fullPlaylist);
                }
            }
        }

        return allPlaylists;
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
