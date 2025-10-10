using API.DTO;

namespace API.Controllers.InterfacesManagers;

/// <summary>
/// Interface operations for PlaylistManager
/// </summary>
public interface IPlaylistManager
{
    /// <summary>
    /// Check the access token and if the token is valid. If the token is valid, it gives all tracks from a playlist
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="playlistId">Playlist ID</param>
    /// <param name="offset">current item (track) to read at start</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Playlist and its items</returns>
    public Task<PlaylistTracksDTO> GetTracksByPlaylist(string sessionId, string playlistId, int? offset,
        CancellationToken ct = default);

}