using Application.Interfaces;
using Application.Interfaces.repositories;
using Domain;
using Infrastructure.DatabaseContext;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureDI
{
    public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
    {
       services.Configure<SpotifySettings>(configuration.GetSection("SpotifySettings"));

        services.AddDbContext<SpotifyDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpClient(
            "SpotifyClient",
            client =>
            {
                client.BaseAddress = new Uri("https://accounts.spotify.com");
            }
        );
        services.AddScoped<IPlaylistService, PlaylistService>();
        services.AddScoped<ISpotifyAuthClient, SpotifyAuthClientService>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<ISpotifyClientFactory, SpotifyClientFactory>();
        services.AddScoped<ISpotifyUserService, SpotifyUserService>();
        services.AddScoped<ISpotifyTrackService, SpotifyTrackService>();
        return services;
    }
}
