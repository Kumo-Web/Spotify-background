using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<SpotifyUser?> GetAsync(string spotifyUserId, CancellationToken ct = default);

    Task SaveAsync(SpotifyUser user, CancellationToken ct = default);
}
