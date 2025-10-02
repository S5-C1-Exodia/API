using API.Controllers.InterfacesManagers;

namespace API.Controllers;

using System;
using System.Threading.Tasks;
using DTO;
using Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

[ApiController]
[Route("api/spotify")]
public class SpotifyController(IAuthManager authManager) : ControllerBase
{
    private readonly IAuthManager _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));

    /// <summary>
    /// Starts the Spotify PKCE authentication and returns the authorization URL and state.
    /// </summary>
    /// <param name="request">The request containing the scopes requested by the mobile client.</param>
    /// <returns>
    /// An <see cref="ActionResult{AuthStartResponseDto}"/> containing the authorization URL and state.
    /// Returns 400 if the request is invalid, 500 for unexpected errors.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the request is invalid.</exception>
    /// <exception cref="Exception">Thrown for unexpected errors.</exception>
    [HttpPost("auth/start")]
    public async Task<ActionResult<AuthStartResponseDto>> StartAuth([FromBody] AuthStartRequestDto request)
    {
        if (request == null)
        {
            return BadRequest("Body is required.");
        }

        try
        {
            AuthStartResponseDto response = await _authManager.StartAuthAsync(request.Scopes);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            // Option: logger ici, ou laisser un middleware global gérer
            return StatusCode(500, "Unexpected error.");
        }
    }

    /// <summary>
    /// Spotify OAuth callback (code and state). Creates the session and redirects to the deeplink (swipez://...).
    /// </summary>
    /// <param name="code">The Spotify authorization code.</param>
    /// <param name="state">The PKCE state.</param>
    /// <param name="device">Optional: device info (fallback if header is missing).</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that redirects to the mobile app deeplink.
    /// Returns 400 for invalid state or arguments, 502 for token exchange failure, 500 for unexpected errors.
    /// </returns>
    /// <exception cref="InvalidStateException">Thrown if the state is unknown or expired.</exception>
    /// <exception cref="TokenExchangeFailedException">Thrown if the token exchange fails.</exception>
    /// <exception cref="ArgumentException">Thrown for invalid arguments.</exception>
    /// <exception cref="Exception">Thrown for unexpected errors.</exception>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, [FromQuery] string device = "")
    {
        // Récupération du deviceInfo: priorité à l'en-tête X-Device-Info, sinon query "device"
        string deviceInfo = device ?? string.Empty;
        StringValues headerValues;
        bool hasHeader = Request.Headers.TryGetValue("X-Device-Info", out headerValues);
        if (hasHeader)
        {
            string headerDevice = headerValues.ToString();
            if (!string.IsNullOrEmpty(headerDevice))
            {
                deviceInfo = headerDevice;
            }
        }

        try
        {
            string deeplink = await _authManager.HandleCallbackAsync(code, state, deviceInfo);

            // Redirection 302 par défaut vers le deeplink de l'app mobile
            return Redirect(deeplink);
        }
        catch (InvalidStateException ex)
        {
            // state inconnu/expiré → 400
            return BadRequest(ex.Message);
        }
        catch (TokenExchangeFailedException)
        {
            // Problème d'échange token (réseau / code invalide) → 502
            return StatusCode(502, "Token exchange failed.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            // Option: logger ici, ou middleware global
            return StatusCode(500, "Unexpected error.");
        }
    }
}