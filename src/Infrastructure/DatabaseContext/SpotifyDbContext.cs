using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseContext;

public class SpotifyDbContext : DbContext
{
    public DbSet<SpotifyToken> SpotifyTokens { get; set; }
    public DbSet<User> Users { get; set; }
}
