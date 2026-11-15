using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace API.Controllers;

/// <summary>
/// The current user's Spotify data.
/// <para>
/// Every route here is about "me". None takes a user id, so there is no route through which one
/// user can read another's data — the previous <c>getUserInfo/{id}</c> shape was exactly that
/// vulnerability.
/// </para>
/// </summary>
[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController(ISpotifyUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var profile = await userService.GetProfileAsync(ct);

        // Projected deliberately rather than returned raw: no need to forward Spotify's full
        // payload (including href/uri/images internals) to the browser.
        return Ok(new
        {
            id = profile.Id,
            displayName = profile.DisplayName,
            images = profile.Images?.Select(i => new { i.Url, i.Height, i.Width }),
            followers = profile.Followers?.Total,
        });
    }

    [HttpGet("top-artists")]
    public async Task<IActionResult> GetTopArtists(
        [FromQuery] string range = "medium_term",
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (!TryParseTimeRange(range, out var timeRange))
        {
            return Problem(
                title: "Invalid time range",
                detail: "range must be one of: short_term, medium_term, long_term.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var artists = await userService.GetTopArtistsAsync(timeRange, Math.Clamp(limit, 1, 50), ct);

        return Ok(artists.Select(a => new
        {
            a.Id,
            a.Name,
            a.Genres,
            image = a.Images?.FirstOrDefault()?.Url,
        }));
    }

    [HttpGet("top-tracks")]
    public async Task<IActionResult> GetTopTracks(
        [FromQuery] string range = "medium_term",
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (!TryParseTimeRange(range, out var timeRange))
        {
            return Problem(
                title: "Invalid time range",
                detail: "range must be one of: short_term, medium_term, long_term.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var tracks = await userService.GetTopTracksAsync(timeRange, Math.Clamp(limit, 1, 50), ct);

        return Ok(tracks.Select(t => new
        {
            t.Id,
            t.Name,
            artists = t.Artists.Select(a => a.Name),
            album = t.Album.Name,
            t.DurationMs,
        }));
    }

    [HttpGet("now-playing")]
    public async Task<IActionResult> GetNowPlaying(CancellationToken ct)
    {
        var playing = await userService.GetCurrentlyPlayingAsync(ct);

        if (playing?.Item is not FullTrack track)
        {
            // Also covers podcasts, local files, and private-session playback.
            return Ok(new { isPlaying = false });
        }

        return Ok(new
        {
            isPlaying = playing.IsPlaying,
            trackId = track.Id,
            title = track.Name,
            artists = track.Artists.Select(a => a.Name),
            album = track.Album.Name,
            albumArt = track.Album.Images?.FirstOrDefault()?.Url,
            progressMs = playing.ProgressMs,
            durationMs = track.DurationMs,
        });
    }

    private static bool TryParseTimeRange(string value, out PersonalizationTopRequest.TimeRange range)
    {
        switch (value)
        {
            case "short_term":
                range = PersonalizationTopRequest.TimeRange.ShortTerm;
                return true;
            case "medium_term":
                range = PersonalizationTopRequest.TimeRange.MediumTerm;
                return true;
            case "long_term":
                range = PersonalizationTopRequest.TimeRange.LongTerm;
                return true;
            default:
                range = PersonalizationTopRequest.TimeRange.MediumTerm;
                return false;
        }
    }
}
