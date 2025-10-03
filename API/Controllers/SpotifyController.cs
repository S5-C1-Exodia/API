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
        // Plus de try/catch ici : si l’AuthManager lance ArgumentException ou autre, le middleware les mappe
        AuthStartResponseDto response = await this._authManager.StartAuthAsync(request.Scopes);
        return this.Ok(response);
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
        // Récupère device info : priorité à l’en-tête X-Device-Info
        string deviceInfo = device ?? string.Empty;
        bool hasHeader = this.Request.Headers.TryGetValue("X-Device-Info", out StringValues headerValues);
        if (hasHeader && !string.IsNullOrEmpty(headerValues.ToString()))
        {
            deviceInfo = headerValues.ToString();
        }

        string deeplink = await this._authManager.HandleCallbackAsync(code, state, deviceInfo);
        return this.Redirect(deeplink);
    }
}