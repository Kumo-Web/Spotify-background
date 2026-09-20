using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/playlists")]
[Authorize]
public sealed class PlaylistsController(
    IPlaylistService playlistService,
    ILogger<PlaylistsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var playlists = await playlistService.GetMyPlaylistsAsync(Math.Clamp(limit, 1, 200), ct);

        return Ok(playlists.Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            trackCount = p.Tracks?.Total ?? 0,
            image = p.Images?.FirstOrDefault()?.Url,
            isPublic = p.Public,
            owner = p.Owner?.DisplayName,
        }));
    }

    [HttpGet("{playlistId}")]
    public async Task<IActionResult> Get(string playlistId, CancellationToken ct)
    {
        var playlist = await playlistService.GetAsync(playlistId, ct);

        return Ok(new
        {
            playlist.Id,
            playlist.Name,
            playlist.Description,
            trackCount = playlist.Tracks?.Total ?? 0,
            image = playlist.Images?.FirstOrDefault()?.Url,
        });
    }

    /// <summary>
    /// Removes a playlist from the user's library.
    /// <para>
    /// Spotify has no delete operation — this unfollows, which is what "delete" does in the
    /// Spotify clients too. The playlist itself continues to exist.
    /// </para>
    /// </summary>
    [HttpDelete("{playlistId}")]
    public async Task<IActionResult> Remove(string playlistId, CancellationToken ct)
    {
        // Authorisation is implicit and total: the client is built for the session's user, so a
        // playlist id belonging to someone else simply fails at Spotify. There is no id we could
        // be tricked into acting on behalf of.
        await playlistService.UnfollowAsync(playlistId, ct);

        logger.LogInformation("Removed playlist {PlaylistId} from the user's library.", playlistId);

        return NoContent();
    }
}
