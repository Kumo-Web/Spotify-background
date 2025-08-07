using System.Text.Json.Serialization;

namespace Domain.Entities;

public class SpotifyToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
}
