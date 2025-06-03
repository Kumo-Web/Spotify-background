using Application.Interfaces;
using Application.Interfaces.repositories;
using Application.Services;
using Domain.Entities;
using Moq;

namespace ApplicationTests;

[TestClass]
public class TokenServiceTests
{
    [TestMethod]
    public async Task GetValidAccessTokenAsync_ShouldRefresh_WhenTokenIsExpired()
    {
        var receivedAtExpired = DateTime.UtcNow.AddSeconds(-3601);
        int expiresInSeconds = 3600;
        var userId = Guid.NewGuid();
        //arrange
        var expiredToken = new SpotifyToken
        {
            AccessToken = "old_token",
            RefreshToken = "refresh_token",
            TokenType = "Bearer",
            Scope = "Scopes",
            ReceivedAt = receivedAtExpired,
            ExpiresIn = expiresInSeconds
        };

        var RefreshdToken = new SpotifyToken
        {
            AccessToken = "new_token",
            TokenType = "Bearer",
            Scope = "Scopes",
            RefreshToken = "refresh_token",
            ReceivedAt = DateTime.UtcNow,
            ExpiresIn = 3600
        };

        var mockSpotifyClient = new Mock<ISpotifyAuthClient>();
        var mockTokenRepository = new Mock<ITokenRepository>();

        mockSpotifyClient
            .Setup(x => x.RefreshAccessTokenAsync("refresh_token"))
            .ReturnsAsync(RefreshdToken);

        mockTokenRepository
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expiredToken);

        var service = new TokenService(mockSpotifyClient.Object, mockTokenRepository.Object);

        var result = await service.GetValidAccessTokenAsync(userId);

        Assert.AreEqual("new_token", result.AccessToken);
    }
}
