using Domain.Entities;

namespace Domain.Interfaces;

public interface ISpotifyAuthClient
{
    Task<SpotifyToken> RefreshAccessTokenAsync(string refreshToken);
    Task<SpotifyToken> GetAccessTokenWithCode(string code);
}
