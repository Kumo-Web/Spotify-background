using Infrastructure.Interfaces;
using SpotifyAPI.Web;

namespace Infrastructure.Services;

public class SpotifyTrackService : ISpotifyTrackService
{
    public Task<List<bool>> CheckSavedTracksAsync(Guid userId, List<string> trackIds)
    {
        throw new NotImplementedException();
    }

    public Task<FullTrack> GetTrackAsync(string trackId)
    {
        throw new NotImplementedException();
    }

    public Task<TrackAudioFeatures> GetTrackAudioFeaturesAsync(string trackId)
    {
        throw new NotImplementedException();
    }

    public Task<List<FullTrack>> GetTracksAsync(List<string> trackIds)
    {
        throw new NotImplementedException();
    }

    public Task<List<TrackAudioFeatures>> GetTracksAudioFeaturesAsync(List<string> trackIds)
    {
        throw new NotImplementedException();
    }

    public Task RemoveSavedTracksAsync(Guid userId, List<string> trackIds)
    {
        throw new NotImplementedException();
    }

    public Task SaveTracksAsync(Guid userId, List<string> trackIds)
    {
        throw new NotImplementedException();
    }
}
