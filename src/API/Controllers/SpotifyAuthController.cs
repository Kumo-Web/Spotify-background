using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyAuthController : ControllerBase
    {
        private readonly ISpotifyAuthClient _spotifyAuthClient;

        public SpotifyAuthController(ISpotifyAuthClient spotifyAuthClient)
        {
            _spotifyAuthClient = spotifyAuthClient;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user id");
            }

            var user = Guid.Parse(userId);
            var url = _spotifyAuthClient.GetAuthorizationUrl(user);
            return Ok(url);
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

            return Ok(token);
        }
    }
}
