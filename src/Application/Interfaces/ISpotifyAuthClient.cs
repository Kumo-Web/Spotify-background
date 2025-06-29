using Domain.Entities;

namespace Application.Interfaces;

public interface ISpotifyAuthClient
{
    Uri GetAuthorizationUrl(Guid userId);
    Task<SpotifyToken> GetAccessTokenWithCode(string code);
    Task<SpotifyToken> RefreshAccessTokenAsync(string refreshToken);
}
