using SpotifyAPI.Web;

namespace Infrastructure.Interfaces;

public interface ISpotifyTrackService
{
    Task<FullTrack> GetTrackAsync(string trackId);
    Task<List<FullTrack>> GetTracksAsync(List<string> trackIds);
    Task<TrackAudioFeatures> GetTrackAudioFeaturesAsync(string trackId);
    Task<List<TrackAudioFeatures>> GetTracksAudioFeaturesAsync(List<string> trackIds);
    Task<List<bool>> CheckSavedTracksAsync(Guid userId, List<string> trackIds);
    Task SaveTracksAsync(Guid userId, List<string> trackIds);
    Task RemoveSavedTracksAsync(Guid userId, List<string> trackIds);
}
