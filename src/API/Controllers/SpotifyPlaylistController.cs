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

    [HttpGet("getUserInfo/{Id}")]
    public async Task<IActionResult> GetSpotifyUserInfo([FromRoute]Guid Id)
    {
        var user = await _playlistService.GetSpotifyUserInfoAsync(Id);
        return Ok(user);
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
