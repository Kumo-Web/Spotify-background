namespace Domain;

public sealed class SpotifySettings
{
    public const string SectionName = "SpotifySettings";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Must exactly match a redirect URI registered in the Spotify dashboard.</summary>
    public string RedirectUrl { get; set; } = string.Empty;

    /// <summary>
    /// OAuth scopes to request. Previously a single space-delimited string that was never
    /// configured, so the authorize URL was built with an empty <c>scope</c> and the resulting
    /// token could not read anything.
    /// </summary>
    public string[] Scopes { get; set; } =
    [
        "user-read-private",
        "user-top-read",
        "user-read-recently-played",
        "user-library-read",
        "user-follow-read",
        "user-read-playback-state",
        "user-read-currently-playing",
        "playlist-read-private",
        "playlist-modify-private",
        "playlist-modify-public",
    ];

    /// <summary>
    /// Default market for track resolution, since <c>GET /me</c> no longer returns country.
    /// </summary>
    public string DefaultMarket { get; set; } = "GB";

    /// <summary>
    /// Send PKCE (<c>code_challenge</c> / <c>code_verifier</c>) alongside the token request.
    /// <para>
    /// Spotify documents PKCE and confidential-client auth as two separate flows: its PKCE
    /// tutorial puts <c>client_id</c> in the body with no secret, and its standard
    /// Authorization Code tutorial uses Basic auth and never mentions <c>code_challenge</c>.
    /// Combining them is undocumented rather than forbidden, and PKCE parameters are additive,
    /// so we send both and get defence in depth: the secret authenticates the client, PKCE binds
    /// the code to this specific login attempt.
    /// </para>
    /// <para>
    /// If Spotify ever rejects the extra parameters, set this false. The single-use server-side
    /// <c>state</c> remains the CSRF protection either way, and that is the control that was
    /// genuinely missing before.
    /// </para>
    /// </summary>
    public bool UsePkce { get; set; } = true;

    /// <summary>
    /// Authenticate the token request with the client secret over HTTP Basic.
    /// Leave true: this app holds its secret server-side, so it is a confidential client and
    /// should prove it. Only set false to run as a public PKCE-only client.
    /// </summary>
    public bool UseClientSecret { get; set; } = true;

    public string ScopeString => string.Join(' ', Scopes);
}
