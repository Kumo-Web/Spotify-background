namespace Domain.Entities;

/// <summary>
/// A stored Spotify OAuth token pair.
/// <para>
/// This is a persistence entity only — it is deliberately NOT the shape of Spotify's token
/// endpoint response. Deserialising the wire response straight into the stored entity (the
/// previous design) coupled our storage schema to Spotify's JSON and made it easy to
/// overwrite good state with absent fields.
/// </para>
/// <para>
/// <see cref="AccessToken"/> and <see cref="RefreshToken"/> hold ciphertext once persisted;
/// the repository protects and unprotects them so plaintext never reaches storage.
/// </para>
/// </summary>
public sealed class SpotifyToken
{
    public required string SpotifyUserId { get; init; }

    public required string AccessToken { get; set; }

    /// <summary>
    /// Long-lived. Spotify frequently omits <c>refresh_token</c> from refresh responses, so this
    /// must be preserved rather than overwritten when a refresh returns nothing for it.
    /// </summary>
    public required string RefreshToken { get; set; }

    public string TokenType { get; set; } = "Bearer";

    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Absolute expiry, computed once at the moment the token is received.
    /// <para>
    /// Stored absolute rather than as Spotify's relative <c>expires_in</c> plus a received-at
    /// timestamp. That older shape allowed the received-at to be left unset, which silently
    /// made every token look permanently expired — triggering a refresh on every single request.
    /// An absolute instant cannot be half-populated.
    /// </para>
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Whether the token is expired, or close enough that it should be refreshed now.
    /// </summary>
    /// <param name="skew">
    /// Safety margin so a token doesn't expire mid-flight between our check and Spotify's.
    /// </param>
    public bool IsExpired(TimeSpan skew) => DateTimeOffset.UtcNow >= ExpiresAt - skew;
}
