using Domain.Entities;

namespace Application.Interfaces;

public interface ISpotifyAuthClient
{
    Task<SpotifyToken> RefreshAccessTokenAsync(string refreshToken);
    Task<SpotifyToken> GetAccessTokenWithCode(string code);
    string GetAuthourizeUrl();
}
