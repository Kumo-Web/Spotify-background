namespace Domain.Entities;

public class User
{
    public Guid UserId { get; set; } = new Guid();
    public string Email { get; set; } = string.Empty;
    public string PassWordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    ///TODO: add relationship to SpotifyToken
    // public virtual SpotifyToken? SpotifyToken { get; set; }
}
