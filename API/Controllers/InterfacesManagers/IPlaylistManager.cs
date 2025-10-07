using API.DTO;

namespace API.Controllers.InterfacesManagers;

public interface IPlaylistManager
{
    
    /// <summary>
    /// Check the access token and if it's valid. If the token is valid, it gives all items from the playlist
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="playlistId">Playlist ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Playlist and its items</returns>
    public Task<PlaylistTracksDTO> GetTracksByPlaylist(string sessionId, string playlistId,
        CancellationToken ct = default);

}