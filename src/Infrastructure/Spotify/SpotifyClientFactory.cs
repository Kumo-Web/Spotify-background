using Application.Interfaces;
using Infrastructure.Interfaces;
using SpotifyAPI.Web;

namespace Infrastructure.Spotify;

internal sealed class SpotifyClientFactory(
    ITokenService tokenService,
    ICurrentUserAccessor currentUser) : ISpotifyClientFactory
{
    public async Task<SpotifyClient> CreateAsync(CancellationToken ct = default)
    {
        var accessToken = await tokenService.GetValidAccessTokenAsync(currentUser.SpotifyUserId, ct);

        var config = SpotifyClientConfig
            .CreateDefault(accessToken)
            // Honours Retry-After on 429. Spotify's Development Mode quota is shared per
            // developer account, so backing off correctly matters more than it used to.
            .WithRetryHandler(new SimpleRetryHandler());

        return new SpotifyClient(config);
    }
}
