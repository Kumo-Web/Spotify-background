using Application.Interfaces.repositories;
using Domain.Entities;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly SpotifyDbContext _context;

    public TokenRepository(SpotifyDbContext context)
    {
        _context = context;
    }

    public async Task<SpotifyToken?> GetByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return null;

        var userToken = await _context.SpotifyTokens.FirstOrDefaultAsync(x => x.UserId == userId);

        if (userToken is null)
            return null;

        return userToken;
    }

    public async Task SaveAsync(SpotifyToken token)
    {
        if (token is null)
            throw new ArgumentNullException(nameof(token));

        _context.SpotifyTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SpotifyToken token)
    {
        if (token is null)
            throw new ArgumentNullException(nameof(token));

        // Try to load the existing token by the primary key (UserId)
        var existing = await _context.SpotifyTokens.FirstOrDefaultAsync(t => t.UserId == token.UserId);

        if (existing is null)
        {
            // If it doesn't exist, add as new
            _context.SpotifyTokens.Add(token);
        }
        else
        {
            // Update scalar properties
            existing.AccessToken = token.AccessToken;
            existing.RefreshToken = token.RefreshToken;
            existing.ExpiresIn = token.ExpiresIn;
            existing.TokenType = token.TokenType;
            existing.Scope = token.Scope;
            existing.ReceivedAt = token.ReceivedAt;
        }

        await _context.SaveChangesAsync();
    }
}
