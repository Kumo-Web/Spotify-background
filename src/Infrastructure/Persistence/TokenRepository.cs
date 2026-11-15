using Application.Interfaces.Repositories;
using Application.Interfaces.Security;
using Domain.Entities;

namespace Infrastructure.Persistence;

/// <summary>
/// Persists Spotify tokens, encrypting the secrets on the way in and decrypting on the way out,
/// so plaintext credentials never reach storage.
/// </summary>
internal sealed class TokenRepository(ICosmosStore store, ITokenProtector protector) : ITokenRepository
{
    /// <summary>Stored shape. Field names say "cipher" so a plaintext write is obvious in review.</summary>
    internal sealed record TokenDocument(
        string AccessTokenCipher,
        string RefreshTokenCipher,
        string TokenType,
        string Scope,
        DateTimeOffset ExpiresAt,
        DateTimeOffset ReceivedAt);

    public async Task<SpotifyToken?> GetAsync(string spotifyUserId, CancellationToken ct = default)
    {
        var result = await store.ReadAsync<TokenDocument>(spotifyUserId, DocumentIds.Token, ct);
        if (result is null)
        {
            return null;
        }

        var doc = result.Value.Value;
        return new SpotifyToken
        {
            SpotifyUserId = spotifyUserId,
            AccessToken = protector.Unprotect(doc.AccessTokenCipher),
            RefreshToken = protector.Unprotect(doc.RefreshTokenCipher),
            TokenType = doc.TokenType,
            Scope = doc.Scope,
            ExpiresAt = doc.ExpiresAt,
            ReceivedAt = doc.ReceivedAt,
        };
    }

    public Task SaveAsync(SpotifyToken token, CancellationToken ct = default)
    {
        var doc = new TokenDocument(
            AccessTokenCipher: protector.Protect(token.AccessToken),
            RefreshTokenCipher: protector.Protect(token.RefreshToken),
            TokenType: token.TokenType,
            Scope: token.Scope,
            ExpiresAt: token.ExpiresAt,
            ReceivedAt: token.ReceivedAt);

        return store.UpsertAsync(
            token.SpotifyUserId, DocumentIds.Token, DocumentTypes.Token, doc, ttlSeconds: null, ct: ct);
    }

    public Task DeleteAsync(string spotifyUserId, CancellationToken ct = default)
        => store.DeleteAsync(spotifyUserId, DocumentIds.Token, ct: ct);
}
