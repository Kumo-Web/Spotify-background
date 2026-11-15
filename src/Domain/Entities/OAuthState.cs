namespace Domain.Entities;

/// <summary>
/// A single-use OAuth authorisation attempt, held server-side between the redirect to Spotify
/// and the callback.
/// <para>
/// This exists to fix a real vulnerability: the previous flow used the caller-supplied user id
/// as the OAuth <c>state</c>, which made it predictable, attacker-chosen and reusable — no CSRF
/// protection at all. State is now high-entropy, server-generated, and consumed on first use.
/// </para>
/// <para>Expiry is enforced by Cosmos TTL, so no cleanup job is needed.</para>
/// </summary>
public sealed class OAuthState
{
    /// <summary>High-entropy random value sent to Spotify as the <c>state</c> parameter.</summary>
    public required string State { get; init; }

    /// <summary>PKCE code verifier. Never leaves the server.</summary>
    public required string CodeVerifier { get; init; }

    /// <summary>Where to send the browser after a successful login. Validated against an allowlist.</summary>
    public string? ReturnPath { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Seconds until Cosmos deletes this document.</summary>
    public int TimeToLiveSeconds { get; init; } = 300;
}
