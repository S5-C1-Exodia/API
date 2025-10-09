namespace API.DTO;

/// <summary>
/// Adaptive DTO for the response in JSON from Spotify API
/// </summary>
public class SpotifyTrackItem
{
    /// <summary>
    /// Get tracks object
    /// </summary>
    public SpotifyTrack Track { get; set; }
}