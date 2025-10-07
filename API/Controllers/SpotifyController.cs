using API.Controllers.InterfacesManagers;
using API.DTO;
using API.Errors;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller for handling Spotify authentication, session management, playlist retrieval, and user preferences.
/// Provides endpoints for starting authentication, handling OAuth callbacks, logging out, retrieving playlists, and managing playlist preferences.
/// </summary>
[ApiController]
[Route("api/spotify")]
public class SpotifyController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly IUserDataManager _userDataManager;
    private readonly IPreferencesManager _preferences;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpotifyController"/> class.
    /// </summary>
    /// <param name="authManager">Service for authentication management.</param>
    /// <param name="userDataManager">Service for user data management.</param>
    /// <param name="preferences">Service for managing user playlist preferences.</param>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
    public SpotifyController(IAuthManager authManager, IUserDataManager userDataManager, IPreferencesManager preferences)
    {
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _userDataManager = userDataManager ?? throw new ArgumentNullException(nameof(userDataManager));
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
    }

    /// <summary>
    /// Starts the Spotify authentication process and returns the authorization URL and state.
    /// </summary>
    /// <param name="request">Request containing the list of scopes to request.</param>
    /// <returns>An <see cref="AuthStartResponseDto"/> with the authorization URL and state.</returns>
    /// <response code="200">Returns the authorization URL and state.</response>
    /// <response code="400">If the scopes are invalid.</response>
    /// <exception cref="ArgumentException">Thrown if scopes are null or empty.</exception>
    [HttpPost("auth/start")]
    [ProducesResponseType(typeof(AuthStartResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AuthStartResponseDto>> StartAuth([FromBody] AuthStartRequestDto request)
    {
        AuthStartResponseDto response = await _authManager.StartAuthAsync(request.Scopes);
        return Ok(response);
    }

    /// <summary>
    /// Handles the OAuth callback, exchanges the code for tokens, and returns a deep link for the authenticated session.
    /// </summary>
    /// <param name="code">Authorization code received from Spotify.</param>
    /// <param name="state">State parameter to validate the request.</param>
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
    /// <param name="sessionId">Session identifier to log out.</param>
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
    /// <returns>A paginated list of playlists as <see cref="PlaylistPageDto"/>.</returns>
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

    // --- Preferences endpoints ---

    /// <summary>
    /// Gets the user's playlist preferences.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user's playlist preferences as <see cref="PlaylistPreferencesDto"/>.</returns>
    [HttpGet("playlist-preferences")]
    public async Task<ActionResult<PlaylistPreferencesDto>> GetPreferences(
        [FromHeader(Name = "X-Session-Id")] string sessionId,
        CancellationToken ct)
    {
        List<string> ids = await _preferences.GetSelectionAsync(sessionId, ct);
        PlaylistPreferencesDto dto = new PlaylistPreferencesDto { PlaylistIds = ids };
        return Ok(dto);
    }

    /// <summary>
    /// Replaces the user's playlist preferences with the provided list.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="body">DTO containing the new playlist IDs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("playlist-preferences")]
    public async Task<IActionResult> ReplacePreferences(
        [FromHeader(Name = "X-Session-Id")] string sessionId,
        [FromBody] PlaylistPreferencesDto body,
        CancellationToken ct)
    {
        List<string> ids = body?.PlaylistIds ?? [];
        await _preferences.ReplaceSelectionAsync(sessionId, ids, ct);
        return NoContent();
    }

    /// <summary>
    /// Adds the provided playlist IDs to the user's preferences.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="body">DTO containing playlist IDs to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPatch("playlist-preferences/add")]
    public async Task<IActionResult> AddPreferences(
        [FromHeader(Name = "X-Session-Id")] string sessionId,
        [FromBody] PlaylistPreferencesDto body,
        CancellationToken ct)
    {
        List<string> ids = body?.PlaylistIds ?? [];
        await _preferences.AddToSelectionAsync(sessionId, ids, ct);
        return NoContent();
    }

    /// <summary>
    /// Removes the provided playlist IDs from the user's preferences.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="body">DTO containing playlist IDs to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPatch("playlist-preferences/remove")]
    public async Task<IActionResult> RemovePreferences(
        [FromHeader(Name = "X-Session-Id")] string sessionId,
        [FromBody] PlaylistPreferencesDto body,
        CancellationToken ct)
    {
        var ids = body?.PlaylistIds ?? [];
        await _preferences.RemoveFromSelectionAsync(sessionId, ids, ct);
        return NoContent();
    }

    /// <summary>
    /// Clears all playlist preferences for the user.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("playlist-preferences")]
    public async Task<IActionResult> ClearPreferences(
        [FromHeader(Name = "X-Session-Id")] string sessionId,
        CancellationToken ct)
    {
        await _preferences.ClearSelectionAsync(sessionId, ct);
        return NoContent();
    }
}