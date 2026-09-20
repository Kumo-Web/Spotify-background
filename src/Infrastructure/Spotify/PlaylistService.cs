using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Infrastructure.Spotify;

internal sealed class PlaylistService(
    ISpotifyClientFactory clientFactory,
    ILogger<PlaylistService> logger) : IPlaylistService
{
    /// <summary>Spotify accepts at most 100 track URIs per playlist write.</summary>
    private const int MaxUrisPerRequest = 100;

    private const int MaxPageSize = 50;

    public async Task<IReadOnlyList<FullPlaylist>> GetMyPlaylistsAsync(
        int maxItems = 200, CancellationToken ct = default)
    {
        var client = await clientFactory.CreateAsync(ct);

        var firstPage = await client.Playlists.CurrentUsers(
            new PlaylistCurrentUsersRequest { Limit = MaxPageSize }, ct);

        if (firstPage.Items is null or { Count: 0 })
        {
            return [];
        }

        var collected = new List<FullPlaylist>();

        // The previous implementation looped while Next was set but re-called CurrentUsers()
        // with no offset, so it returned page one repeatedly. The SDK paginator handles offsets.
        await foreach (var playlist in client.Paginate(firstPage, cancel: ct))
        {
            collected.Add(playlist);

            if (collected.Count >= maxItems)
            {
                break;
            }
        }

        return collected;
    }

    public async Task<FullPlaylist> GetAsync(string playlistId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);

        var client = await clientFactory.CreateAsync(ct);
        return await client.Playlists.Get(playlistId, ct);
    }

    public async Task<string> CreateAsync(
        string name, string? description = null, bool isPublic = false, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var client = await clientFactory.CreateAsync(ct);

        // POST /me/playlists. The userId-taking overload targets POST /users/{id}/playlists,
        // which Spotify removed in February 2026 and the SDK marks obsolete.
        var request = new PlaylistCreateRequest(name)
        {
            Public = isPublic,
            Collaborative = false,
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            request.Description = description;
        }

        var created = await client.Playlists.Create(request, ct);

        return created.Id
            ?? throw new InvalidOperationException("Spotify created a playlist but returned no id.");
    }

    public async Task AddTracksAsync(
        string playlistId, IReadOnlyList<string> trackUris, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);

        if (trackUris.Count == 0)
        {
            return;
        }

        var client = await clientFactory.CreateAsync(ct);

        foreach (var batch in Batch(trackUris))
        {
            // POST /playlists/{id}/items — the /tracks endpoint was removed.
            await client.Playlists.AddPlaylistItems(
                playlistId, new PlaylistAddItemsRequest(batch), ct);
        }

        logger.LogInformation(
            "Added {Count} tracks to playlist in {Batches} request(s).",
            trackUris.Count, (trackUris.Count + MaxUrisPerRequest - 1) / MaxUrisPerRequest);
    }

    public async Task RemoveTracksAsync(
        string playlistId, IReadOnlyList<string> trackUris, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);

        if (trackUris.Count == 0)
        {
            return;
        }

        var client = await clientFactory.CreateAsync(ct);

        foreach (var batch in Batch(trackUris))
        {
            // DELETE /playlists/{id}/items, whose request body shape also changed — hence the
            // V2 request type rather than the obsolete one.
            var request = new PlaylistRemoveItemsRequestV2
            {
                Items = [.. batch.Select(uri => new PlaylistRemoveItemsRequestV2.Item { Uri = uri })],
            };

            await client.Playlists.RemovePlaylistItems(playlistId, request, ct);
        }
    }

    public async Task ReplaceTracksAsync(
        string playlistId, IReadOnlyList<string> trackUris, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);

        var client = await clientFactory.CreateAsync(ct);

        var batches = Batch(trackUris).ToList();

        // Replace sets the whole contents, so only the first batch can replace; the rest append.
        // Replacing with an empty list is the correct way to clear a playlist.
        await client.Playlists.ReplacePlaylistItems(
            playlistId, new PlaylistReplaceItemsRequest([.. batches.FirstOrDefault() ?? []]), ct);

        foreach (var batch in batches.Skip(1))
        {
            await client.Playlists.AddPlaylistItems(
                playlistId, new PlaylistAddItemsRequest(batch), ct);
        }
    }

    public async Task UpdateDetailsAsync(
        string playlistId, string? name = null, string? description = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);

        if (name is null && description is null)
        {
            return;
        }

        var client = await clientFactory.CreateAsync(ct);

        var request = new PlaylistChangeDetailsRequest();
        if (name is not null)
        {
            request.Name = name;
        }

        if (description is not null)
        {
            request.Description = description;
        }

        await client.Playlists.ChangeDetails(playlistId, request, ct);
    }

    public async Task UnfollowAsync(string playlistId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);

        var client = await clientFactory.CreateAsync(ct);

        // DELETE /me/library. February 2026 consolidated every unsave/unfollow endpoint here,
        // including DELETE /playlists/{id}/followers, and this one takes URIs rather than ids.
        await client.Library.RemoveItems(
            new LibraryRemoveItemsRequest([$"spotify:playlist:{playlistId}"]), ct);

        logger.LogInformation("Unfollowed playlist {PlaylistId}.", playlistId);
    }

    private static IEnumerable<List<string>> Batch(IReadOnlyList<string> uris)
    {
        for (var i = 0; i < uris.Count; i += MaxUrisPerRequest)
        {
            yield return [.. uris.Skip(i).Take(MaxUrisPerRequest)];
        }
    }
}
