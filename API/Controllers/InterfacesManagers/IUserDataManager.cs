using API.DTO;

namespace API.Controllers.InterfacesManagers
{
    /// <summary>
    /// Orchestrates access to Spotify user data (tokens, cache, API calls) for read operations.
    /// </summary>
    public interface IUserDataManager
    {
        /// <summary>
        /// Returns a paginated page of playlists for the given session.
        /// Uses database cache first; on cache miss, calls Spotify API, persists the page, and returns it.
        /// Refreshes the access token transparently if required.
        /// </summary>
        /// <param name="sessionId">Opaque application session identifier.</param>
        /// <param name="pageToken">Opaque page token for pagination (null for the first page).</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PlaylistPageDto> GetPlaylistsAsync(string sessionId, string? pageToken, CancellationToken ct = default);
    }
}