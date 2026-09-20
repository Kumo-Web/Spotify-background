namespace Domain.Entities;

/// <summary>
/// An application user. Spotify is the only identity provider, so the Spotify user id is the
/// identity — there is no local password, role or email.
/// <para>
/// Email and product/country are deliberately absent: Spotify's February 2026 API changes
/// removed <c>email</c>, <c>country</c>, <c>product</c> and <c>explicit_content</c> from
/// <c>GET /me</c>, so they can no longer be read from the profile. Market is configured.
/// </para>
/// </summary>
public sealed class SpotifyUser
{
    /// <summary>Spotify's own user id. Primary key and Cosmos partition key.</summary>
    public required string SpotifyUserId { get; init; }

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-2 market used for track resolution. Configured rather than detected,
    /// because <c>GET /me</c> no longer returns <c>country</c>.
    /// </summary>
    public string Market { get; set; } = "GB";

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastLoginAt { get; set; } = DateTimeOffset.UtcNow;
}
