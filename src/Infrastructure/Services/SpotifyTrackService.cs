using Infrastructure.Interfaces;
using SpotifyAPI.Web;

namespace Infrastructure.Services;

public class SpotifyTrackService : ISpotifyTrackService
{
    private readonly ISpotifyClientFactory _spotifyClientFactory;

    public SpotifyTrackService(ISpotifyClientFactory spotifyClientFactory)
    {
        _spotifyClientFactory = spotifyClientFactory;
    }

    public async Task<List<bool>> CheckSavedTracksAsync(Guid userId, List<string> trackIds)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (trackIds == null || trackIds.Count == 0)
            throw new ArgumentNullException(nameof(trackIds));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var request = new LibraryCheckTracksRequest(trackIds);
        var result = await spotifyClient.Library.CheckTracks(request);

        return result;
    }

    public async Task<FullTrack> GetTrackAsync(Guid userId, string trackId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(trackId))
            throw new ArgumentNullException(nameof(trackId));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var track = await spotifyClient.Tracks.Get(trackId);

        return track;
    }

    public async Task<TrackAudioFeatures> GetTrackAudioFeaturesAsync(Guid userId, string trackId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(trackId))
            throw new ArgumentNullException(nameof(trackId));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var audioFeatures = await spotifyClient.Tracks.GetAudioFeatures(trackId);

        return audioFeatures;
    }

    public async Task<List<FullTrack>> GetTracksAsync(Guid userId, List<string> trackIds)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (trackIds == null || trackIds.Count == 0)
            throw new ArgumentNullException(nameof(trackIds));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var request = new TracksRequest(trackIds);
        var tracks = await spotifyClient.Tracks.GetSeveral(request);

        return tracks.Tracks;
    }

    public async Task<List<TrackAudioFeatures>> GetTracksAudioFeaturesAsync(Guid userId, List<string> trackIds)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (trackIds == null || trackIds.Count == 0)
            throw new ArgumentNullException(nameof(trackIds));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var request = new TracksAudioFeaturesRequest(trackIds);
        var audioFeatures = await spotifyClient.Tracks.GetSeveralAudioFeatures(request);

        return audioFeatures.AudioFeatures;
    }

    public async Task RemoveSavedTracksAsync(Guid userId, List<string> trackIds)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (trackIds == null || trackIds.Count == 0)
            throw new ArgumentNullException(nameof(trackIds));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var request = new LibraryRemoveTracksRequest(trackIds);
        await spotifyClient.Library.RemoveTracks(request);
    }

    public async Task SaveTracksAsync(Guid userId, List<string> trackIds)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));
        if (trackIds == null || trackIds.Count == 0)
            throw new ArgumentNullException(nameof(trackIds));

        var spotifyClient = await _spotifyClientFactory.CreateSpotifyClient(userId);

        var request = new LibrarySaveTracksRequest(trackIds);
        await spotifyClient.Library.SaveTracks(request);
    }
}
