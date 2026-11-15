using SpotifyAPI.Web;

namespace Infrastructure.Interfaces;

/// <summary>
/// Produces an authenticated <see cref="SpotifyClient"/> for the current user.
/// <para>
/// Note there is no user-id parameter. The client is always built for whoever the request is
/// authenticated as, resolved from <see cref="Application.Interfaces.ICurrentUserAccessor"/>.
/// That is deliberate: the previous signature took a caller-supplied id, which let any caller
/// obtain a client acting as any user.
/// </para>
/// </summary>
public interface ISpotifyClientFactory
{
    Task<SpotifyClient> CreateAsync(CancellationToken ct = default);
}
