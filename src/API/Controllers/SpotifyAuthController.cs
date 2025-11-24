using Application.Interfaces;
using Application.Interfaces.repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyAuthController : ControllerBase
    {
        private readonly ISpotifyAuthClient _spotifyAuthClient;
        private readonly ITokenRepository _tokenRepository;

        public SpotifyAuthController(ISpotifyAuthClient spotifyAuthClient, ITokenRepository tokenRepository)
        {
            _spotifyAuthClient = spotifyAuthClient;
            _tokenRepository = tokenRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user id");
            }
            var user = Guid.Parse(userId);

            var url = _spotifyAuthClient.GetAuthorizationUrl(user);

            return Content(url.ToString(), "text/plain");
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            var code = Request.Query["code"];
            var state = Request.Query["state"];

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return BadRequest("Invalid code or state");
            }

            var token = await _spotifyAuthClient.GetAccessTokenWithCode(code);

            if (token == null)
            {
                return BadRequest("Failed to retrieve access token");
            }

            var user = await _tokenRepository.GetByUserIdAsync(Guid.Parse(state));

            await _tokenRepository.UpdateAsync(token);

            var frontendUrl = "http://localhost:3000";

            return Redirect($"{frontendUrl}/dashboard?access_token={Uri.EscapeDataString(token.AccessToken)}");

        }
    }
}
