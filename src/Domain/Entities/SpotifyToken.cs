namespace Domain.Entities;

public class SpotifyToken
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
}
