namespace API.DTO
{
    /// <summary>
    /// Represents a paginated result page of playlists.
    /// </summary>
    public class PlaylistPageDto
    {
        /// <summary>
        /// Items of the current page.
        /// </summary>
        public List<PlaylistItemDto> Items { get; init; } = new();

        /// <summary>
        /// Opaque token to retrieve the next page.
        /// <br/>
        /// <c>null</c> if there is no next page.
        /// </summary>
        public string? NextPageToken { get; init; }
    }
}