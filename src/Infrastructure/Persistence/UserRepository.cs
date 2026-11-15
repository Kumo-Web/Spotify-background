using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistence;

internal sealed class UserRepository(ICosmosStore store) : IUserRepository
{
    internal sealed record UserDocument(
        string DisplayName,
        string Market,
        DateTimeOffset CreatedAt,
        DateTimeOffset LastLoginAt);

    public async Task<SpotifyUser?> GetAsync(string spotifyUserId, CancellationToken ct = default)
    {
        var result = await store.ReadAsync<UserDocument>(spotifyUserId, DocumentIds.User, ct);
        if (result is null)
        {
            return null;
        }

        var doc = result.Value.Value;
        return new SpotifyUser
        {
            SpotifyUserId = spotifyUserId,
            DisplayName = doc.DisplayName,
            Market = doc.Market,
            CreatedAt = doc.CreatedAt,
            LastLoginAt = doc.LastLoginAt,
        };
    }

    public Task SaveAsync(SpotifyUser user, CancellationToken ct = default)
    {
        var doc = new UserDocument(user.DisplayName, user.Market, user.CreatedAt, user.LastLoginAt);

        return store.UpsertAsync(
            user.SpotifyUserId, DocumentIds.User, DocumentTypes.User, doc, ttlSeconds: null, ct: ct);
    }
}
