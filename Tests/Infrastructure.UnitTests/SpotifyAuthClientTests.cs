using Domain;
using Infrastructure.Services;
using Microsoft.CodeCoverage.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace InfrastructureTests;

[TestClass]
public class SpotifyAuthClientTests
{
    [TestMethod]
    public void GetAuthourizeUrlAsync_ShouldReturnUrl()
    {
        // Arrange

        var scope = "tests scopes";
        var clientId = "client_id";
        var clientSecret = "client_secret";
        var redirectUri = "http://localhost:3000/callback";
        var userIdGuid = Guid.NewGuid();

        var testSettings = new SpotifySettings
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            RedirectUrl = redirectUri,
            Scopes = scope
        };

        var expectedQuery = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "response_type", "code" },
            { "redirect_uri", redirectUri },
            { "scope", scope },
            { "state", userIdGuid.ToString() }
        };

        var queryString = string.Join(
            "&",
            expectedQuery.Select(
                kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"
            )
        );

        var loggerMock = new Mock<ILogger<SpotifyAuthClientService>>();
        var mockOptions = new Mock<IOptionsSnapshot<SpotifySettings>>();
        var mockhttpClient = new Mock<IHttpClientFactory>();
        mockOptions.Setup(x => x.Value).Returns(testSettings);

        var expectedUrl = new Uri($"https://accounts.spotify.com/authorize?{queryString}");

        var service = new SpotifyAuthClientService(
            mockOptions.Object,
            loggerMock.Object,
            mockhttpClient.Object
        );

        // Act
        var result = service.GetAuthorizationUrl(userIdGuid);

        // Assert
        Assert.AreEqual(expectedUrl.AbsoluteUri, result.AbsoluteUri);
    }
}
