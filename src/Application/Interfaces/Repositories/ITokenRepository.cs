using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ITokenRepository
{
    /// <summary>Returns the stored token with tokens decrypted, or null if none exists.</summary>
    Task<SpotifyToken?> GetAsync(string spotifyUserId, CancellationToken ct = default);

    /// <summary>Writes the token, encrypting the secrets before they reach storage.</summary>
    Task SaveAsync(SpotifyToken token, CancellationToken ct = default);

    Task DeleteAsync(string spotifyUserId, CancellationToken ct = default);
}
