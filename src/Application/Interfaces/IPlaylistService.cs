
namespace Application.Interfaces;

public interface IPlaylistService
{
    Task CreatePlaylistAsync(Guid userId, string playlistName, List<string> trackUris);
    Task<List<string>> GetMostPlayedTracksAsync(Guid userId, int limit);
    Task<string> GetSpotifyUserInfoAsync(Guid userId);
    Task<List<string>> GetTopArtistsAsync(Guid userId, int limit);
    Task<List<string>> GetTopTracksAsync(Guid userId, int limit);
}
