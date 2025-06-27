using Domain.Entities;

namespace Application.Interfaces;

public interface ISpotifyAuthClient
{
    string GetAuthorizationUrl();
    Task<SpotifyToken> GetAccessTokenWithCode(string code);
<<<<<<< Updated upstream
    string GetAuthourizeUrl();
=======
    Task<SpotifyToken> RefreshAccessTokenAsync(string code);
>>>>>>> Stashed changes
}
