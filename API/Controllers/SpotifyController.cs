using API.Controllers.InterfacesManagers;
using API.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace API.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyController(IAuthManager authManager) : ControllerBase
{
    private readonly IAuthManager _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));

    /// <summary>
    /// Starts the Spotify PKCE authentication and returns the authorization URL and state.
    /// </summary>
    /// <param name="request">The request containing the scopes requested by the mobile client.</param>
    /// <returns>Authorization URL and state for the OAuth flow.</returns>
    [HttpPost("auth/start")]
    public async Task<ActionResult<AuthStartResponseDto>> StartAuth([FromBody] AuthStartRequestDto request)
    {
        AuthStartResponseDto response = await _authManager.StartAuthAsync(request.Scopes);
        return Ok(response);
    }

    /// <summary>
    /// Spotify OAuth callback (code and state). Creates the session and redirects to the deeplink (swipez://...).
    /// </summary>
    /// <param name="code">The Spotify authorization code.</param>
    /// <param name="state">The PKCE state.</param>
    /// <param name="device">Optional: device info fallback if header is missing.</param>
    /// <returns>Redirect (302) to the mobile app deeplink.</returns>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state,
        [FromQuery] string? device = "")
    {
        string deviceInfo = device ?? string.Empty;
        bool hasHeader = Request.Headers.TryGetValue("X-Device-Info", out StringValues headerValues);
        if (hasHeader && !string.IsNullOrEmpty(headerValues.ToString()))
        {
            deviceInfo = headerValues.ToString();
        }

        string deeplink = await _authManager.HandleCallbackAsync(code, state, deviceInfo);
        return Redirect(deeplink);
    }

    /// <summary>
    /// Disconnects the user from Spotify by revoking tokens and clearing session data.
    /// </summary>
    /// <param name="sessionId">The current session identifier provided by the mobile client in the header X-Session-Id.</param>
    /// <returns>204 No Content if successful (idempotent).</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromHeader(Name = "X-Session-Id")] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest("Missing X-Session-Id");

        await _authManager.LogoutAsync(sessionId);
        return NoContent();
    }
}