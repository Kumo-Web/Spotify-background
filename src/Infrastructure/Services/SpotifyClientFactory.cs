using Application.Interfaces;
using Infrastructure.Interfaces;
using SpotifyAPI.Web;

namespace Infrastructure.Services;

public class SpotifyClientFactory : ISpotifyClientFactory
{
    private readonly ITokenService _tokenService;

    public SpotifyClientFactory(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }
    public async Task<SpotifyClient> CreateSpotifyClient(Guid userId)
    {
        var token = await _tokenService.GetValidAccessTokenAsync(userId);
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        var config = SpotifyClientConfig
            .CreateDefault(token.AccessToken);
         return new SpotifyClient(config);
    }
}
