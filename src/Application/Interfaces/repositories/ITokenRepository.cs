using Domain.Entities;

namespace Application.Interfaces.repositories;

public interface ITokenRepository
{
    Task<SpotifyToken?> GetByUserIdAsync(Guid userId);
    Task SaveAsync(SpotifyToken token);
}
