using Application.Interfaces;
using Moq;

namespace Application.UnitTests;

[TestClass]
public class SpotifyClientAuthServiceTests
{

    [TestMethod]
    public async Task GetAuthourizeUrlAsync_ShouldReturnUrl()
    {

        var scope = "tests scopes";
        var clientId = "client_id";
        var clientSecret = "client_secret";
        var resdirectUri = "http://localhost:3000/callback";
        var state = "state";

        var spotifyMock = new Mock<ISpotifyAuthClient>();
    }
}
