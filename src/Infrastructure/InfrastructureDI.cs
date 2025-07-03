using Application.Interfaces;
using Application.Interfaces.repositories;
using Domain;
using Infrastructure.DatabaseContext;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureDI
{
    public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
    {
       services.Configure<SpotifySettings>(configuration.GetSection("SpotifySettings"));

        services.AddDbContext<SpotifyDbContext>();

        services.AddHttpClient(
            "SpotifyClient",
            client =>
            {
                client.BaseAddress = new Uri("https://accounts.spotify.com");
            }
        );
        services.AddScoped<ISpotifyAuthClient, SpotifyAuthClientService>();
        services.AddScoped<ITokenRepository, TokenRepository>();

        return services;
    }
}
