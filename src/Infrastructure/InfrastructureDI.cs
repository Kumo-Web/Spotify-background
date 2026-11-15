using System.Net.Http;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Security;
using Application.Services;
using Azure.Core;
using Azure.Identity;
using Domain;
using Domain.Configuration;
using Infrastructure.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Infrastructure.Spotify;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureDI
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfiguration(configuration);
        services.AddPersistence();
        services.AddSpotify();

        services.AddSingleton<ITokenProtector, DataProtectionTokenProtector>();
        services.AddSingleton<IOAuthChallengeFactory, OAuthChallengeFactory>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    private static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // ValidateOnStart so misconfiguration fails at boot with a clear message rather than
        // producing subtly broken behaviour at runtime. The previous setup had no ClientId and no
        // Scopes configured, so it silently built authorize URLs with an empty scope parameter
        // and issued tokens that could not read anything.
        services.AddOptions<SpotifySettings>()
            .Bind(configuration.GetSection(SpotifySettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.ClientId),
                "SpotifySettings:ClientId is required.")
            .Validate(s => !s.UseClientSecret || !string.IsNullOrWhiteSpace(s.ClientSecret),
                "SpotifySettings:ClientSecret is required when UseClientSecret is true. Set it via user secrets, never appsettings.")
            .Validate(s => Uri.TryCreate(s.RedirectUrl, UriKind.Absolute, out _),
                "SpotifySettings:RedirectUrl must be an absolute URL, and must exactly match a redirect URI registered in the Spotify dashboard.")
            .Validate(s => s.Scopes.Length > 0,
                "SpotifySettings:Scopes must not be empty, or the resulting token can read nothing.")
            .ValidateOnStart();

        services.AddOptions<CosmosSettings>()
            .Bind(configuration.GetSection(CosmosSettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Endpoint),
                "Cosmos:Endpoint is required.")
            .ValidateOnStart();

        services.AddOptions<FrontendSettings>()
            .Bind(configuration.GetSection(FrontendSettings.SectionName))
            .Validate(s => s.AllowedOrigins.Length > 0,
                "Frontend:AllowedOrigins must list at least one origin; CORS is an allowlist, not a wildcard.")
            .ValidateOnStart();

        services.AddOptions<DataProtectionSettings>()
            .Bind(configuration.GetSection(DataProtectionSettings.SectionName));
    }

    private static void AddPersistence(this IServiceCollection services)
    {
        // Managed identity in Azure; the local emulator uses the account key instead.
        services.AddSingleton<TokenCredential>(_ => new DefaultAzureCredential());

        services.AddSingleton(provider =>
        {
            var settings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosSettings>>().Value;
            var credential = provider.GetRequiredService<TokenCredential>();
            return CosmosClientFactory.Create(settings, credential);
        });

        services.AddSingleton<ICosmosStore, CosmosStore>();

        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOAuthStateRepository, OAuthStateRepository>();
    }

    private static void AddSpotify(this IServiceCollection services)
    {
        // Named client with the accounts base address. The previous code registered a named
        // client then called CreateClient() with no name, so the configuration was never used.
        services.AddHttpClient(SpotifyAuthClient.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://accounts.spotify.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<ISpotifyAuthClient, SpotifyAuthClient>();
        services.AddScoped<ISpotifyClientFactory, SpotifyClientFactory>();
        services.AddScoped<ISpotifyUserService, SpotifyUserService>();
        services.AddScoped<IPlaylistService, PlaylistService>();
    }

    /// <summary>
    /// Creates the Cosmos database and container if they don't exist. Call once during startup.
    /// <para>
    /// Deliberately fails startup if Cosmos is unreachable — an API that boots without its
    /// database only fails later, per request, and more confusingly.
    /// </para>
    /// </summary>
    public static async Task InitialiseCosmosAsync(
        IServiceProvider provider, CancellationToken ct = default)
    {
        var settings = provider
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosSettings>>().Value;
        var client = provider.GetRequiredService<CosmosClient>();

        try
        {
            await CosmosClientFactory.EnsureCreatedAsync(client, settings, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or CosmosException)
        {
            // Replace the SDK's 30-line retry stack trace with something that says what to do.
            throw new InvalidOperationException(
                $"""
                Could not reach Cosmos DB at {settings.Endpoint}.

                If running locally, start the emulator first:
                  docker run --detach --name cosmos \
                    -p 8081:8081 -p 8080:8080 -p 1234:1234 \
                    mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest \
                    --protocol https

                --protocol https is required: the emulator defaults to HTTP and the .NET SDK
                cannot speak it. Wait for http://localhost:8080/ready before starting the API.

                If running in Azure, check that Cosmos:Endpoint is correct and that this app's
                managed identity holds the Cosmos Data Contributor role.
                """,
                ex);
        }
    }
}
