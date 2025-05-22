using Application.Helper;
using Application.Interfaces;
using Application.Interfaces.repositories;
using Domain.Entities;

namespace Application.Services;

public class TokenService : ITokenService
{
    private readonly ISpotifyAuthClient _spotifyClient;
    private readonly ITokenRepository _tokenRepository;

    public TokenService(ISpotifyAuthClient spotifyClient, ITokenRepository tokenRepository)
    {
        _tokenRepository =
            tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        _spotifyClient = spotifyClient ?? throw new ArgumentNullException(nameof(spotifyClient));
    }

    public async Task<SpotifyToken> GetValidAccessTokenAsync(Guid userId)
    {
        var token = await _tokenRepository.GetByUserIdAsync(userId);

        if (token is null)
            throw new ArgumentNullException(nameof(token));

        DateTime expirationUtc = TokenHelper.CalculateTokenExpiryTime(
            token.ReceivedAt,
            token.ExpiresIn
        );

        if (TokenHelper.IsTokenExpired(expirationUtc))
        {
            var refreshedToken = await _spotifyClient.RefreshAccessTokenAsync(token.RefreshToken);
            await _tokenRepository.SaveAsync(refreshedToken);
            return refreshedToken;
        }
        return token;
    }
}
