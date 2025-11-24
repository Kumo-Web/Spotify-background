
using SpotifyAPI.Web;

namespace Application.Interfaces;

public interface IPlaylistService
{
    Task<string> CreatePlaylistAsync(Guid userId, string playlistName, string description = null, bool isPublic = false);
    Task<FullPlaylist> GetPlaylistAsync(Guid userId, string playlistId);
    Task<List<FullPlaylist>> GetUserPlaylistsAsync(Guid userId, int limit = 50);
    Task AddTracksToPlaylistAsync(Guid userId, string playlistId, List<string> trackUris);
    Task RemoveTracksFromPlaylistAsync(Guid userId, string playlistId, List<string> trackUris);
    Task UpdatePlaylistDetailsAsync(Guid userId, string playlistId, string name, string description);
    Task DeletePlaylistAsync(Guid userId, string playlistId);
    
}
