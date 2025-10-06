using API.DTO;

namespace API.Managers.InterfacesHelpers
{
    /// <summary>
    /// Thin HTTP wrapper over Spotify Web API for data retrieval (no token logic).
    /// </summary>
    public interface ISpotifyApiHelper
    {
        /// <summary>
        /// Calls GET /v1/me/playlists (paginated) and maps the response to <see cref="PlaylistPageDto"/>.
        /// </summary>
        /// <param name="accessToken">Bearer access token.</param>
        /// <param name="pageToken">Opaque page token (maybe the Spotify "next" URL or an offset token).</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PlaylistPageDto> GetPlaylistsAsync(string accessToken, string? pageToken, CancellationToken ct = default);
    }
}