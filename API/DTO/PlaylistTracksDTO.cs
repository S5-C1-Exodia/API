namespace API.DTO;

/// <summary>
/// Representing a playlist with its items
/// </summary>
public class PlaylistTracksDTO
{
    /// <summary>
    /// ID of the playlist
    /// </summary>
    public string PlaylistId { get; set; }
    
    /// <summary>
    /// Limit of track imported
    /// </summary>
    public int Limit { get; set; }
    
    /// <summary>
    /// List of tracks in the playlist
    /// <remarks>Contains the album and the artist of a track</remarks>
    /// </summary>
    public List<TrackDTO> Tracks { get; set; }
}