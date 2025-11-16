using System.Linq.Expressions;
using Application.Helper;
using Application.Interfaces;
using Application.Interfaces.repositories;
using Domain.Entities;

namespace Application.Services;

public class TokenService : ITokenService
{
    private readonly ISpotifyAuthClient _spotifyClient;
    private readonly ITokenRepository _tokenRepository;
    private readonly ISpotifyAuthClient _spotifyAuthClient;

    public TokenService(ISpotifyAuthClient spotifyClient, ITokenRepository tokenRepository, ISpotifyAuthClient spotifyAuthClient)
    {
        _tokenRepository =
            tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        _spotifyAuthClient = spotifyAuthClient;
        _spotifyClient = spotifyClient ?? throw new ArgumentNullException(nameof(spotifyClient));
    }

    public async Task<SpotifyToken> GetValidAccessTokenAsync(Guid userId)
    {
        try
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
                var refreshedToken = await _spotifyClient.RefreshAccessTokenAsync(
                    token.RefreshToken
                );
                //TODO remove this once state is done
                refreshedToken.UserId = token.UserId;
                await _tokenRepository.UpdateAsync(refreshedToken);
                return refreshedToken;
            }
            return token;
        }
        catch (ArgumentNullException ex)
        {
            return null;
        }
    }
}
