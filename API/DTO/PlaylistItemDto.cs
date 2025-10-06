namespace API.DTO
{
    /// <summary>
    /// Represents a Spotify playlist displayable on the client side.
    /// </summary>
    public sealed class PlaylistItemDto
    {
        /// <summary>
        /// Unique identifier of the playlist at the provider (e.g., Spotify).
        /// </summary>
        public string PlaylistId { get; init; } = default!;

        /// <summary>
        /// Human-readable name of the playlist.
        /// </summary>
        public string Name { get; init; } = default!;

        /// <summary>
        /// URL of a representative image for the playlist, if available.
        /// </summary>
        public string? ImageUrl { get; init; }

        /// <summary>
        /// Display name of the playlist owner, if available.
        /// </summary>
        public string? Owner { get; init; }

        /// <summary>
        /// Number of tracks contained in the playlist, if known.
        /// </summary>
        public int? TrackCount { get; init; }

        /// <summary>
        /// Indicates whether the playlist is part of the user's selection.
        /// Defaults to <c>false</c>; will be set during merge with the selection.
        /// </summary>
        public bool Selected { get; init; } = false;
    }
}