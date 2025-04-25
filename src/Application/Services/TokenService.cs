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
        _tokenRepository = tokenRepository;
        _spotifyClient = spotifyClient ?? throw new ArgumentNullException(nameof(spotifyClient));
    }
    public Task<SpotifyToken> GetValidAccessTokenAsync(SpotifyToken token)
    {
        if (token == null)
            return null;
        

    }
}
