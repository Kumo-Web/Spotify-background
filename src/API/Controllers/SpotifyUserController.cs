using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpotifyUserController : Controller
{
    private readonly ILogger<SpotifyUserController> _logger;
    private readonly ISpotifyUserService _userService;

    public SpotifyUserController(ILogger<SpotifyUserController> logger, ISpotifyUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet("getUserInfo/{Id}")]
    public async Task<IActionResult> GetSpotifyUserInfo([FromRoute] Guid Id)
    {
        var user = await _userService.GetCurrentUserAsync(Id);
        return Ok(user);
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error!");
    }
}
