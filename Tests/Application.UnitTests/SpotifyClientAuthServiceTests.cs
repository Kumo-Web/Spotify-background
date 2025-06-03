namespace Application.UnitTests;

[TestClass]
public class SpotifyClientAuthServiceTests
{
    [TestMethod]
    public async Task GetAuthourizeUrlAsync_ShouldReturnUrl()
    {
        var spotifyMock = new Mock<ISpotifyAuthClient>();
        var tokenService = new TokenService(spotifyMock.Object, null);
        var result = await tokenService.GetAuthourizeUrlAsync();
        Assert.AreEqual("https://accounts.spotify.com/authorize", result);
    }
}
