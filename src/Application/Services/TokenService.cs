using System.Collections.Concurrent;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Hands out valid Spotify access tokens, refreshing transparently when needed.
/// </summary>
public sealed class TokenService(
    ISpotifyAuthClient authClient,
    ITokenRepository tokenRepository,
    ILogger<TokenService> logger) : ITokenService
{
    /// <summary>
    /// Refresh this long before nominal expiry, so a token can't expire in flight between our
    /// check and Spotify receiving the request.
    /// </summary>
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromSeconds(60);

    /// <summary>
    /// One refresh at a time per user.
    /// <para>
    /// Without this, a page firing several requests at once would run concurrent refreshes with
    /// the same refresh token. Spotify may rotate the refresh token on use, so the slower call
    /// can end up persisting a token that has already been superseded — or failing outright.
    /// </para>
    /// <para>
    /// In-process, which is sufficient because the API runs as a single scale-to-zero replica.
    /// If it is ever scaled out, this needs to become a distributed lock or an ETag-conditional
    /// write; the repository already supports the latter.
    /// </para>
    /// </summary>
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> RefreshLocks = new();

    public async Task<string> GetValidAccessTokenAsync(
        string spotifyUserId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spotifyUserId);

        var token = await tokenRepository.GetAsync(spotifyUserId, ct)
            ?? throw new SpotifyNotConnectedException(spotifyUserId, "no token is stored");

        if (!token.IsExpired(ExpirySkew))
        {
            return token.AccessToken;
        }

        var gate = RefreshLocks.GetOrAdd(spotifyUserId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            // Re-read inside the lock: another request may have refreshed while we queued, in
            // which case there is nothing left to do.
            token = await tokenRepository.GetAsync(spotifyUserId, ct)
                ?? throw new SpotifyNotConnectedException(spotifyUserId, "token disappeared during refresh");

            if (!token.IsExpired(ExpirySkew))
            {
                return token.AccessToken;
            }

            return await RefreshAndStoreAsync(token, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<string> RefreshAndStoreAsync(SpotifyToken existing, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(existing.RefreshToken))
        {
            throw new SpotifyNotConnectedException(
                existing.SpotifyUserId, "the stored refresh token is empty; the account must be re-linked");
        }

        logger.LogInformation("Refreshing expired Spotify access token.");

        var grant = await authClient.RefreshAsync(existing.RefreshToken, ct);

        var refreshed = new SpotifyToken
        {
            SpotifyUserId = existing.SpotifyUserId,
            AccessToken = grant.AccessToken,

            // The bug this replaces: Spotify usually omits refresh_token on a refresh response,
            // and the old code copied the absent value over the stored one — blanking it, so the
            // account silently disconnected after its first refresh. Keep what we have unless
            // Spotify actually hands us a new one.
            RefreshToken = string.IsNullOrWhiteSpace(grant.RefreshToken)
                ? existing.RefreshToken
                : grant.RefreshToken,

            TokenType = grant.TokenType,

            // Spotify also omits scope on refresh; the granted scope hasn't changed.
            Scope = string.IsNullOrWhiteSpace(grant.Scope) ? existing.Scope : grant.Scope,

            // Absolute expiry computed here, once, from a known-good clock reading.
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(grant.ExpiresInSeconds),
            ReceivedAt = DateTimeOffset.UtcNow,
        };

        await tokenRepository.SaveAsync(refreshed, ct);

        return refreshed.AccessToken;
    }
}
