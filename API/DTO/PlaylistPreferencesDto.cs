namespace API.DTO
{
    /// <summary>
    /// Represents the playlist selection on the backend side.
    /// </summary>
    public class PlaylistPreferencesDto
    {
        /// <summary>
        /// List of selected playlist identifiers.
        /// </summary>
        public List<string> PlaylistIds { get; init; } = new();
    }
}