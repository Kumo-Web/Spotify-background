using System.Text.Json;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
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
        //arrange
        var expiredToken = new SpotifyToken
        {
            AccessToken = "old_token",
            RefreshToken = "refresh_toke",
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

        mockSpotifyClient
            .Setup(x => x.RefreshAccessTokenAsync("refresh_token"))
            .ReturnsAsync(RefreshdToken);

        var service = new TokenService(mockSpotifyClient.Object);

        var result = await service.GetValidAccessTokenAsync(expiredToken);

        Assert.AreEqual("new_token", result.AccessToken);
    }
}
