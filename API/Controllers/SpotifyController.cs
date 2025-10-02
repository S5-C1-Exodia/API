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
    /// Démarre l'auth PKCE Spotify et renvoie l'URL d'autorisation + le state.
    /// </summary>
    /// <param name="request">Scopes demandés par le client mobile.</param>
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
    /// Callback OAuth Spotify (code + state). Crée la session et redirige vers le deeplink swipez://...
    /// </summary>
    /// <param name="code">Code d'autorisation Spotify</param>
    /// <param name="state">State PKCE</param>
    /// <param name="device">Optionnel: deviceInfo (fallback si pas d'en-tête)</param>
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