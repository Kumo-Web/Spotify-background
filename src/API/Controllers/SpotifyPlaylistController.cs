using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpotifyPlaylistController : ControllerBase
{
    public SpotifyPlaylistController()
    {

    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> GetSpotifyUserInfo(Guid Id)
    {
        return Ok();
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
