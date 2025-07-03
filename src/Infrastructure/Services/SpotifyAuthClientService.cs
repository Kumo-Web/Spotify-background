using Application.Interfaces;
using Domain;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class SpotifyAuthClientService : ISpotifyAuthClient
{
    private readonly SpotifySettings _settings;
    private readonly ILogger<SpotifyAuthClientService> _logger;
    private readonly HttpClient _httpClient;

    public SpotifyAuthClientService(
        IOptionsSnapshot<SpotifySettings> settings,
        ILogger<SpotifyAuthClientService> logger,
        IHttpClientFactory client
    )
    {
        _logger = logger;
        _httpClient = client.CreateClient();
        _settings = settings.Value;
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

    public async Task<SpotifyToken> GetAccessTokenWithCode(string code)
    {
        if (code is null)
            throw new ArgumentNullException(nameof(code));

        var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://accounts.spotify.com/api/token"
        );

        var content = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "redirtect_uri", _settings.RedirectUrl },
            { "code", code }
        };

        if (_settings.ClientId is null || _settings.ClientSecret is null)
            throw new ArgumentNullException(nameof(_settings.ClientId));

        var clientCredential = $"{_settings.ClientId}:{_settings.ClientSecret}";

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(clientCredential))
        );

        requestMessage.Content = new FormUrlEncodedContent(content);
        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get access token: {error}");
        }

        var contentJson = await response.Content.ReadAsStringAsync();
        var token = System.Text.Json.JsonSerializer.Deserialize<SpotifyToken>(contentJson);
        
        return token;
    }

    public Task<SpotifyToken> RefreshAccessTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }
}
