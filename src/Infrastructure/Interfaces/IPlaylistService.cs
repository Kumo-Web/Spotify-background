
using SpotifyAPI.Web;

namespace Application.Interfaces;

public interface IPlaylistService
{
    Task CreatePlaylistAsync(Guid userId, string playlistName, List<string> trackUris);
    Task<List<string>> GetMostPlayedTracksAsync(Guid userId, int limit);
    Task<PrivateUser> GetSpotifyUserInfoAsync(Guid userId);
    Task<Paging<FullArtist>> GetTopArtistsAsync(Guid userId, int limit);
    Task<List<string>> GetTopTracksAsync(Guid userId, int limit);
}
