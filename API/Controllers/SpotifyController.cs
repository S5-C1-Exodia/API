using API.Controllers.InterfacesManagers;
using API.DTO;
using API.Errors;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller for Spotify authentication and session management.
/// </summary>
[ApiController]
[Route("api/spotify")]
public class SpotifyController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly IUserDataManager _userDataManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpotifyController"/> class.
    /// </summary>
    /// <param name="authManager">The authentication manager.</param>
    /// <param name="userDataManager">The user data manager.</param>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
    public SpotifyController(IAuthManager authManager, IUserDataManager userDataManager)
    {
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _userDataManager = userDataManager ?? throw new ArgumentNullException(nameof(userDataManager));
    }

    /// <summary>
    /// Starts the authentication process and returns the authorization URL and state.
    /// </summary>
    /// <param name="scopes">The list of scopes to request.</param>
    /// <returns>An <see cref="AuthStartResponseDto"/> containing the authorization URL and state.</returns>
    /// <response code="200">Returns the authorization URL and state.</response>
    /// <response code="400">If the scopes are invalid.</response>
    /// <exception cref="ArgumentException">Thrown if scopes is null or empty.</exception>
    [HttpPost("auth/start")]
    [ProducesResponseType(typeof(AuthStartResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AuthStartResponseDto>> StartAuth([FromBody] AuthStartRequestDto request)
    {
        AuthStartResponseDto response = await _authManager.StartAuthAsync(request.Scopes);
        return Ok(response);
    }

    /// <summary>
    /// Handles the OAuth callback, exchanges the code for tokens, and returns a deep link.
    /// </summary>
    /// <param name="code">The authorization code received from the provider.</param>
    /// <param name="state">The state parameter to validate the request.</param>
    /// <param name="deviceInfo">Optional device information.</param>
    /// <returns>A deep link string for the authenticated session.</returns>
    /// <response code="200">Returns the deep link for the authenticated session.</response>
    /// <response code="400">If the code or state is invalid.</response>
    /// <exception cref="ArgumentException">Thrown if code or state is null or empty.</exception>
    /// <exception cref="InvalidStateException">Thrown if the state is unknown or expired.</exception>
    /// <exception cref="TokenExchangeFailedException">Thrown if token exchange fails.</exception>
    [HttpPost("callback")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, [FromQuery] string? deviceInfo)
    {
        var result = await _authManager.HandleCallbackAsync(code, state, deviceInfo);
        return Ok(result);
    }

    /// <summary>
    /// Logs out a user by purging all session-related data and denylisting the refresh token.
    /// </summary>
    /// <param name="sessionId">The session identifier to log out.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <response code="204">Logout successful.</response>
    /// <response code="400">If the sessionId is invalid.</response>
    /// <exception cref="ArgumentException">Thrown if sessionId is null or empty.</exception>
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Logout([FromQuery] string sessionId)
    {

        await _authManager.LogoutAsync(sessionId);
        return NoContent();
    }
    
    /// <summary>
    /// Returns a paginated list of the current user's playlists.
    /// Requires the opaque session id in <c>X-Session-Id</c>.
    /// Optionally accepts <c>X-Page-Token</c> for pagination (opaque token, e.g., Spotify "next" URL).
    /// </summary>
    /// <param name="sessionId">Opaque application session identifier.</param>
    /// <param name="pageToken">Opaque pagination token, or null for the first page.</param>
    [HttpGet("playlists")]
    [ProducesResponseType(typeof(PlaylistPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlaylistPageDto>> GetPlaylists(
        [FromHeader(Name = "X-Session-Id")] string sessionId,
        [FromHeader(Name = "X-Page-Token")] string? pageToken)
    {
        PlaylistPageDto page = await _userDataManager.GetPlaylistsAsync(
            sessionId,
            pageToken,
            HttpContext.RequestAborted
        );

        return Ok(page);
    }

}