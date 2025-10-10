namespace API.DTO;

/// <summary>
/// Internal model representing the tracks information of a Spotify playlist.
/// </summary>
public sealed class SpotifyTracksInfo
{
    /// <summary>
    /// Gets or sets the total number of tracks in the playlist.
    /// </summary>
    public int? Total { get; set; }
    
    /// <summary>
    /// URL to a playlist
    /// </summary>
    public string Href { get; set; } = string.Empty;
}