using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpotifyPlaylistController : ControllerBase
{
    private readonly IPlaylistService _playlistService;
    public SpotifyPlaylistController(IPlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    public async Task<IActionResult> CreatePlaylist()
    {
        return Ok();
    }

    public async Task<IActionResult> GetMostPlayed()
    {
        return Ok();
    }
}
