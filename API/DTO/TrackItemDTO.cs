namespace API.DTO;

/// <summary>
/// Necessary for the props JSON given by Spotify
/// </summary>
public class TrackItemDTO
{
    /// <summary>
    /// Get tracks object
    /// </summary>
    public TrackDTO Track { get; set; }
}