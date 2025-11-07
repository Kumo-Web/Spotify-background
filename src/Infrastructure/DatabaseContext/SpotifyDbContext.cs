using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DatabaseContext;

public class SpotifyDbContext : DbContext
{
    public SpotifyDbContext(DbContextOptions<SpotifyDbContext> options) : base(options)
    {
    }

    public DbSet<SpotifyToken> SpotifyTokens { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SpotifyToken>(entity =>
        {
            entity.HasKey(t => t.UserId);

            modelBuilder.Entity<User>()
            .HasOne(u => u.SpotifyToken)
            .WithOne(t => t.User)
            .HasForeignKey<SpotifyToken>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        });
            
    }
}
