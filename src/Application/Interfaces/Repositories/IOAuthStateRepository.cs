using Domain.Entities;

namespace Application.Interfaces.Repositories;

/// <summary>
/// Short-lived store for in-flight OAuth authorisation attempts.
/// </summary>
public interface IOAuthStateRepository
{
    Task SaveAsync(OAuthState state, CancellationToken ct = default);

    /// <summary>
    /// Atomically fetches and deletes the state, so a given <c>state</c> value can be redeemed
    /// exactly once. A replayed callback finds nothing and is rejected.
    /// </summary>
    /// <returns>The state, or null if unknown, expired, or already consumed.</returns>
    Task<OAuthState?> ConsumeAsync(string state, CancellationToken ct = default);
}
