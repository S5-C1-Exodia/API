namespace API.DTO;

/// <summary>
/// Internal model representing the tracks information of a Spotify playlist.
/// </summary>
public sealed class SpotifyTracks
{
    /// <summary>
    /// Gets or sets the total number of tracks in the playlist.
    /// </summary>
    public int? Total { get; set; }
}