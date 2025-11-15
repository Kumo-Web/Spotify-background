using Application.Interfaces.repositories;
using Domain.Entities;
using Infrastructure.DatabaseContext;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SpotifyDbContext _context;

    public UserRepository(SpotifyDbContext context)
    {
        _context = context;
    }
    public async Task<User> GetUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return null;

        var user = await _context.Users.FindAsync(userId);

        if (user is null)
            return null;
            
        return user;
    }

    public async Task SaveAsync(User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(User user)
    {
        throw new NotImplementedException();
    }
}
