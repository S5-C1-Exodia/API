using API.Controllers.InterfacesManagers;
using API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private IPlaylistManager _playlistManager;

        public PlaylistController(IPlaylistManager playlistManager)
        {
            _playlistManager = playlistManager;
        }
        
        [HttpGet("/playlists/{playlist_id}/tracks")]
        [ProducesResponseType(typeof(PlaylistTracksDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlaylistTracks([FromQuery] string playlistId, [FromQuery] string sessionId, CancellationToken ct)
        {
            var result = await _playlistManager.GetTracksByPlaylist(sessionId, playlistId, ct);
            return Ok(result);
        }
    }
}
