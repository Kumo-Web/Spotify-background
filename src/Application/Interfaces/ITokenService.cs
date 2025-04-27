using Domain.Entities;

namespace Application.Interfaces;

public interface ITokenService
{
    Task<SpotifyToken> GetValidAccessTokenAsync(Guid userId);
}
