using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

internal sealed class OAuthStateRepository(ICosmosStore store, ILogger<OAuthStateRepository> logger)
    : IOAuthStateRepository
{
    internal sealed record OAuthStateDocument(
        string CodeVerifier,
        string? ReturnPath,
        DateTimeOffset CreatedAt);

    public Task SaveAsync(OAuthState state, CancellationToken ct = default)
    {
        var doc = new OAuthStateDocument(state.CodeVerifier, state.ReturnPath, state.CreatedAt);

        return store.UpsertAsync(
            PartitionKeys.PreAuth,
            DocumentIds.OAuthState(state.State),
            DocumentTypes.OAuthState,
            doc,
            // Cosmos deletes this itself, so an abandoned login leaves nothing behind and there
            // is no cleanup job to forget about.
            ttlSeconds: state.TimeToLiveSeconds,
            ct: ct);
    }

    public async Task<OAuthState?> ConsumeAsync(string state, CancellationToken ct = default)
    {
        var id = DocumentIds.OAuthState(state);

        var result = await store.ReadAsync<OAuthStateDocument>(PartitionKeys.PreAuth, id, ct);
        if (result is null)
        {
            // Unknown, expired, or already redeemed — all indistinguishable to a caller, and
            // deliberately so.
            logger.LogWarning("OAuth callback presented a state value that is not on record.");
            return null;
        }

        // Delete conditionally on the ETag we just read. If a replayed callback races us, only
        // one delete succeeds, so the code can only ever be exchanged once.
        var won = await store.DeleteAsync(PartitionKeys.PreAuth, id, result.Value.ETag, ct);
        if (!won)
        {
            logger.LogWarning("OAuth state was consumed concurrently; rejecting this callback as a replay.");
            return null;
        }

        var doc = result.Value.Value;
        return new OAuthState
        {
            State = state,
            CodeVerifier = doc.CodeVerifier,
            ReturnPath = doc.ReturnPath,
            CreatedAt = doc.CreatedAt,
        };
    }
}
