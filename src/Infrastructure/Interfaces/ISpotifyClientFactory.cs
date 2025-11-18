using SpotifyAPI.Web;

namespace Infrastructure.Interfaces;

public interface ISpotifyClientFactory
{
    /// <summary>
    /// Create a SpotifyClient configured for the given user access token.
    /// The caller is responsible for passing a valid (non-expired) access token.
    /// </summary>
    Task<SpotifyClient> CreateSpotifyClient(Guid userId);
}
