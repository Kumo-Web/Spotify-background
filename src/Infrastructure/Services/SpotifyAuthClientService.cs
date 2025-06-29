using Application.Interfaces;
using Domain;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class SpotifyAuthClientService : ISpotifyAuthClient
{
    private readonly SpotifySettings _settings;
    private readonly ILogger<SpotifyAuthClientService> _logger;

    public SpotifyAuthClientService(
        IOptionsSnapshot<SpotifySettings> settings,
        ILogger<SpotifyAuthClientService> logger
    )
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public Task<SpotifyToken> GetAccessTokenWithCode(string code)
    {
        throw new NotImplementedException();
    }

    public Uri GetAuthorizationUrl(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentNullException(nameof(userId));

        var clientId = _settings.ClientId;
        var redirectUri = _settings.RedirectUrl;
        var scope = _settings.Scopes;
        var state = userId.ToString();

        var expectedQuery = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "response_type", "code" },
            { "redirect_uri", redirectUri },
            { "scope", scope },
            { "state", state }
        };

        var queryString = string.Join(
            "&",
            expectedQuery.Select(
                x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"
            )
        );

        return new Uri($"https://accounts.spotify.com/authorize?{queryString}");
    }

    public Task<SpotifyToken> RefreshAccessTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }
}
