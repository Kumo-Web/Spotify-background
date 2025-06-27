using Application.Interfaces;
<<<<<<< Updated upstream
=======
using Application.Services;
>>>>>>> Stashed changes
using Moq;

namespace Application.UnitTests;

[TestClass]
public class SpotifyClientAuthServiceTests
{

    [TestMethod]
    public async Task GetAuthourizeUrlAsync_ShouldReturnUrl()
    {
<<<<<<< Updated upstream

        var scope = "tests scopes";
        var clientId = "client_id";
        var clientSecret = "client_secret";
        var resdirectUri = "http://localhost:3000/callback";
        var state = "state";

        var spotifyMock = new Mock<ISpotifyAuthClient>();
=======
        // Arrange
        var clientId = "client_id";
        var clientSecret = "someClientSecret";
        var expectedUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code";

        var spotifyMock = new Mock<ISpotifyAuthClient>();
        spotifyMock.Setup(x => x.GetAuthorizationUrl()).Returns(expectedUrl);

        var service = new SpotifyClientAuthService(spotifyMock.Object, clientId, clientSecret);

        // Act
        var result = service.GetAuthorizationUrl();

        // Assert
        Assert.AreEqual(expectedUrl, result);
>>>>>>> Stashed changes
    }
}