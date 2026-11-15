using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Models;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests;

[TestClass]
public sealed class TokenServiceTests
{
    private const string UserId = "spotify-user-1";

    private Mock<ISpotifyAuthClient> _authClient = null!;
    private Mock<ITokenRepository> _tokenRepository = null!;
    private TokenService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _authClient = new Mock<ISpotifyAuthClient>();
        _tokenRepository = new Mock<ITokenRepository>();
        _sut = new TokenService(
            _authClient.Object, _tokenRepository.Object, NullLogger<TokenService>.Instance);
    }

    private static SpotifyToken StoredToken(
        DateTimeOffset expiresAt, string refreshToken = "stored-refresh", string scope = "stored-scope")
        => new()
        {
            SpotifyUserId = UserId,
            AccessToken = "stored-access",
            RefreshToken = refreshToken,
            Scope = scope,
            ExpiresAt = expiresAt,
            ReceivedAt = DateTimeOffset.UtcNow.AddHours(-1),
        };

    private void GivenStored(SpotifyToken token)
        => _tokenRepository
            .Setup(r => r.GetAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

    private void GivenRefreshReturns(SpotifyTokenGrant grant)
        => _authClient
            .Setup(c => c.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);

    [TestMethod]
    public async Task ReturnsStoredToken_WhenNotExpired()
    {
        GivenStored(StoredToken(DateTimeOffset.UtcNow.AddMinutes(30)));

        var result = await _sut.GetValidAccessTokenAsync(UserId);

        Assert.AreEqual("stored-access", result);
        _authClient.Verify(
            c => c.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never,
            "A valid token must not trigger a refresh.");
    }

    [TestMethod]
    public async Task Refreshes_WhenExpired()
    {
        GivenStored(StoredToken(DateTimeOffset.UtcNow.AddMinutes(-5)));
        GivenRefreshReturns(new SpotifyTokenGrant("new-access", "new-refresh", 3600, "Bearer", "scope"));

        var result = await _sut.GetValidAccessTokenAsync(UserId);

        Assert.AreEqual("new-access", result);
        _tokenRepository.Verify(r => r.SaveAsync(It.IsAny<SpotifyToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Refreshes_WhenWithinExpirySkew()
    {
        // Not yet expired, but close enough that it could expire in flight.
        GivenStored(StoredToken(DateTimeOffset.UtcNow.AddSeconds(20)));
        GivenRefreshReturns(new SpotifyTokenGrant("new-access", null, 3600, "Bearer", ""));

        var result = await _sut.GetValidAccessTokenAsync(UserId);

        Assert.AreEqual("new-access", result, "A token inside the skew window should be refreshed early.");
    }

    /// <summary>
    /// The regression test for the bug that disconnected the account after its first refresh:
    /// Spotify usually omits refresh_token on a refresh, and the old code copied that absent
    /// value over the stored one, blanking it.
    /// </summary>
    [TestMethod]
    public async Task PreservesStoredRefreshToken_WhenSpotifyOmitsIt()
    {
        GivenStored(StoredToken(DateTimeOffset.UtcNow.AddMinutes(-5), refreshToken: "original-refresh"));
        GivenRefreshReturns(new SpotifyTokenGrant("new-access", RefreshToken: null, 3600, "Bearer", ""));

        SpotifyToken? saved = null;
        _tokenRepository
            .Setup(r => r.SaveAsync(It.IsAny<SpotifyToken>(), It.IsAny<CancellationToken>()))
            .Callback<SpotifyToken, CancellationToken>((t, _) => saved = t)
            .Returns(Task.CompletedTask);

        await _sut.GetValidAccessTokenAsync(UserId);

        Assert.IsNotNull(saved);
        Assert.AreEqual("original-refresh", saved.RefreshToken,
            "The stored refresh token must survive a refresh response that omits it.");
    }

    [TestMethod]
    public async Task PreservesStoredScope_WhenSpotifyOmitsIt()
    {
        GivenStored(StoredToken(DateTimeOffset.UtcNow.AddMinutes(-5), scope: "original-scope"));
        GivenRefreshReturns(new SpotifyTokenGrant("new-access", null, 3600, "Bearer", Scope: ""));

        SpotifyToken? saved = null;
        _tokenRepository
            .Setup(r => r.SaveAsync(It.IsAny<SpotifyToken>(), It.IsAny<CancellationToken>()))
            .Callback<SpotifyToken, CancellationToken>((t, _) => saved = t)
            .Returns(Task.CompletedTask);

        await _sut.GetValidAccessTokenAsync(UserId);

        Assert.AreEqual("original-scope", saved!.Scope);
    }

    [TestMethod]
    public async Task UsesRotatedRefreshToken_WhenSpotifyProvidesOne()
    {
        GivenStored(StoredToken(DateTimeOffset.UtcNow.AddMinutes(-5), refreshToken: "original-refresh"));
        GivenRefreshReturns(new SpotifyTokenGrant("new-access", "rotated-refresh", 3600, "Bearer", "s"));

        SpotifyToken? saved = null;
        _tokenRepository
            .Setup(r => r.SaveAsync(It.IsAny<SpotifyToken>(), It.IsAny<CancellationToken>()))
            .Callback<SpotifyToken, CancellationToken>((t, _) => saved = t)
            .Returns(Task.CompletedTask);

        await _sut.GetValidAccessTokenAsync(UserId);

        Assert.AreEqual("rotated-refresh", saved!.RefreshToken);
    }

    [TestMethod]
    public async Task StoresAbsoluteExpiry_DerivedFromExpiresIn()
    {
        GivenStored(StoredToken(DateTimeOffset.UtcNow.AddMinutes(-5)));
        GivenRefreshReturns(new SpotifyTokenGrant("new-access", null, ExpiresInSeconds: 3600, "Bearer", ""));

        SpotifyToken? saved = null;
        _tokenRepository
            .Setup(r => r.SaveAsync(It.IsAny<SpotifyToken>(), It.IsAny<CancellationToken>()))
            .Callback<SpotifyToken, CancellationToken>((t, _) => saved = t)
            .Returns(Task.CompletedTask);

        var before = DateTimeOffset.UtcNow;
        await _sut.GetValidAccessTokenAsync(UserId);

        var expected = before.AddSeconds(3600);
        Assert.IsTrue((saved!.ExpiresAt - expected).Duration() < TimeSpan.FromSeconds(5),
            $"Expected an absolute expiry near {expected:O} but got {saved.ExpiresAt:O}.");
    }

    [TestMethod]
    public async Task Throws_WhenNoTokenStored()
    {
        _tokenRepository
            .Setup(r => r.GetAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SpotifyToken?)null);

        await Assert.ThrowsExactlyAsync<SpotifyNotConnectedException>(
            () => _sut.GetValidAccessTokenAsync(UserId));
    }

    [TestMethod]
    public async Task Throws_WhenStoredRefreshTokenIsEmpty()
    {
        // The state a previously-affected account would be left in: expired, with no way back.
        GivenStored(StoredToken(DateTimeOffset.UtcNow.AddMinutes(-5), refreshToken: ""));

        await Assert.ThrowsExactlyAsync<SpotifyNotConnectedException>(
            () => _sut.GetValidAccessTokenAsync(UserId));

        _authClient.Verify(
            c => c.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never,
            "Should fail fast rather than call Spotify with an empty refresh token.");
    }

    [TestMethod]
    public async Task Throws_WhenUserIdIsBlank()
        => await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => _sut.GetValidAccessTokenAsync("  "));
}
