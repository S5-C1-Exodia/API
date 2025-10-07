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

        /// <summary>
        /// Verify the access token, take the playlist ID, and it returns all items from playlist
        /// </summary>
        /// <param name="accessToken">valid access token</param>
        /// <param name="playlistId">playlist ID</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>Items from the playlist</returns>
        public Task<PlaylistTracksDTO> GetPlaylistTracks(string accessToken, string playlistId,
            CancellationToken ct = default);
    }
}