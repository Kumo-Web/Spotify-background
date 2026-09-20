using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Infrastructure.Spotify;

internal sealed class SpotifyUserService(
    ISpotifyClientFactory clientFactory,
    ILogger<SpotifyUserService> logger) : ISpotifyUserService
{
    /// <summary>Spotify's own per-page maximum for these endpoints.</summary>
    private const int MaxPageSize = 50;

    public async Task<PrivateUser> GetProfileAsync(CancellationToken ct = default)
    {
        var client = await clientFactory.CreateAsync(ct);

        // Note: as of Spotify's February 2026 changes this no longer carries Country, Email,
        // Product or ExplicitContent. Market comes from configuration instead.
        return await client.UserProfile.Current(ct);
    }

    public async Task<IReadOnlyList<FullArtist>> GetTopArtistsAsync(
        PersonalizationTopRequest.TimeRange range, int limit = 50, CancellationToken ct = default)
    {
        var client = await clientFactory.CreateAsync(ct);

        var page = await client.Personalization.GetTopArtists(
            new PersonalizationTopRequest { TimeRangeParam = range, Limit = Math.Min(limit, MaxPageSize) },
            ct);

        return await CollectAsync(client, page, limit, ct);
    }

    public async Task<IReadOnlyList<FullTrack>> GetTopTracksAsync(
        PersonalizationTopRequest.TimeRange range, int limit = 50, CancellationToken ct = default)
    {
        var client = await clientFactory.CreateAsync(ct);

        var page = await client.Personalization.GetTopTracks(
            new PersonalizationTopRequest { TimeRangeParam = range, Limit = Math.Min(limit, MaxPageSize) },
            ct);

        return await CollectAsync(client, page, limit, ct);
    }

    public async Task<IReadOnlyList<PlayHistoryItem>> GetRecentlyPlayedAsync(
        int limit = 50, CancellationToken ct = default)
    {
        var client = await clientFactory.CreateAsync(ct);

        var page = await client.Player.GetRecentlyPlayed(
            new PlayerRecentlyPlayedRequest { Limit = Math.Min(limit, MaxPageSize) }, ct);

        return page.Items ?? [];
    }

    public async Task<IReadOnlyList<SavedTrack>> GetSavedTracksAsync(
        int maxItems = 200, CancellationToken ct = default)
    {
        var client = await clientFactory.CreateAsync(ct);

        var page = await client.Library.GetTracks(
            new LibraryTracksRequest { Limit = MaxPageSize }, ct);

        return await CollectAsync(client, page, maxItems, ct);
    }

    public async Task<IReadOnlyList<FullArtist>> GetFollowedArtistsAsync(
        int maxItems = 200, CancellationToken ct = default)
    {
        var client = await clientFactory.CreateAsync(ct);

        // Followed artists are cursor-paged under an "artists" envelope, so the generic
        // paginator doesn't apply. Walk the cursor explicitly.
        //
        // The previous implementation looped while Next was non-empty but passed
        // `After = request.After` — never advancing the cursor — so it re-requested the same
        // page forever.
        var collected = new List<FullArtist>();
        string? after = null;

        do
        {
            var response = await client.Follow.OfCurrentUser(
                new FollowOfCurrentUserRequest { Limit = MaxPageSize, After = after }, ct);

            var artists = response.Artists;
            if (artists?.Items is null or { Count: 0 })
            {
                break;
            }

            collected.AddRange(artists.Items);

            // Advance from the response, which is what makes this terminate.
            after = artists.Cursors?.After;
        }
        while (!string.IsNullOrEmpty(after) && collected.Count < maxItems);

        return collected.Count > maxItems ? collected[..maxItems] : collected;
    }

    public async Task<CurrentlyPlaying?> GetCurrentlyPlayingAsync(CancellationToken ct = default)
    {
        var client = await clientFactory.CreateAsync(ct);

        try
        {
            // Returns 204 No Content when nothing is playing, which surfaces as null.
            return await client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(), ct);
        }
        catch (APIException ex)
        {
            logger.LogDebug(ex, "Could not read currently-playing state; treating as nothing playing.");
            return null;
        }
    }

    /// <summary>
    /// Pages through a <see cref="Paging{T}"/> up to <paramref name="maxItems"/> using the SDK's
    /// paginator, which tracks the offset correctly.
    /// <para>
    /// This replaces two hand-rolled loops that were both broken: one re-requested page one
    /// forever because it never advanced the offset, the other never advanced its cursor.
    /// </para>
    /// </summary>
    private static async Task<IReadOnlyList<T>> CollectAsync<T>(
        SpotifyClient client, Paging<T> firstPage, int maxItems, CancellationToken ct)
    {
        if (firstPage.Items is null or { Count: 0 })
        {
            return [];
        }

        var collected = new List<T>();

        // Paginate re-yields the first page's items, so this must not be pre-seeded with them.
        await foreach (var item in client.Paginate(firstPage, cancel: ct))
        {
            collected.Add(item);

            if (collected.Count >= maxItems)
            {
                break;
            }
        }

        return collected;
    }
}
