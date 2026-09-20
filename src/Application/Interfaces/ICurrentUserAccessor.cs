namespace Application.Interfaces;

/// <summary>
/// The identity of whoever this operation is running as.
/// <para>
/// This exists to remove a whole class of vulnerability rather than guard against it. Previously
/// every user-scoped endpoint took a user id from the caller — request body, route segment, or
/// the OAuth <c>state</c> — so any caller could pass any id and act as that user. Services now
/// take no user-id parameter, so there is no longer a channel through which one user can address
/// another's data.
/// </para>
/// <para>
/// Two implementations: one reading the authenticated principal for HTTP requests, one set
/// explicitly per iteration by the background job, which has no HTTP context.
/// </para>
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>The current Spotify user id.</summary>
    /// <exception cref="InvalidOperationException">If no user is established.</exception>
    string SpotifyUserId { get; }

    bool IsAuthenticated { get; }
}
