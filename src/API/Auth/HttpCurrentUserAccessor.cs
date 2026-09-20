using Application.Interfaces;

namespace API.Auth;

/// <summary>
/// Resolves the current user from the authenticated principal on the HTTP context.
/// <para>
/// This is the whole IDOR fix. Identity comes from a signed, HttpOnly session cookie that the
/// client cannot forge, so there is no request-supplied value to tamper with. Contrast the
/// previous design, where the user id arrived in the request body or route.
/// </para>
/// </summary>
internal sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserAccessor
{
    public string SpotifyUserId =>
        httpContextAccessor.HttpContext?.User.FindFirst(SessionClaims.SpotifyUserId)?.Value
        ?? throw new InvalidOperationException(
            "No authenticated user on this request. Endpoints touching user data must require authorisation.");

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.FindFirst(SessionClaims.SpotifyUserId) is not null;
}
