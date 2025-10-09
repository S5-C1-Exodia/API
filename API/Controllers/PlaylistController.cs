using API.Controllers.InterfacesManagers;
using API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for playlists
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private IPlaylistManager _playlistManager;

        public PlaylistController(IPlaylistManager playlistManager)
        {
            _playlistManager = playlistManager;
        }
        
        [HttpGet("/playlists/tracks")]
        [ProducesResponseType(typeof(PlaylistTracksDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlaylistTracks(
            [FromQuery] string playlistId, 
            [FromHeader(Name = "X-Session-Id")] string sessionId, 
            [FromQuery] int? offset,
            CancellationToken ct)
        {
            PlaylistTracksDTO result = await _playlistManager.GetTracksByPlaylist(sessionId, playlistId, offset, ct);
            return Ok(result);
        }
    }
}
