namespace API.Auth;

/// <summary>
/// Claim types in the session cookie. The Spotify user id lives here and nowhere else —
/// never in a request body, route or query string.
/// </summary>
internal static class SessionClaims
{
    public const string SpotifyUserId = "spotify:user_id";
    public const string DisplayName = "spotify:display_name";
}
