using SpotifyAPI.Web;

namespace Infrastructure.Interfaces;

public interface ISpotifyTrackService
{
    Task<FullTrack> GetTrackAsync(Guid userId, string trackId);
    Task<List<FullTrack>> GetTracksAsync(Guid userId, List<string> trackIds);
    Task<TrackAudioFeatures> GetTrackAudioFeaturesAsync(Guid userId, string trackId);
    Task<List<TrackAudioFeatures>> GetTracksAudioFeaturesAsync(Guid userId, List<string> trackIds);
    Task<List<bool>> CheckSavedTracksAsync(Guid userId, List<string> trackIds);
    Task SaveTracksAsync(Guid userId, List<string> trackIds);
    Task RemoveSavedTracksAsync(Guid userId, List<string> trackIds);
}
