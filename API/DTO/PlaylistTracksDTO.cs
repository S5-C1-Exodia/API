namespace API.DTO;

/// <summary>
/// DTO for information from tracks and a playlist
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
    public int? Limit { get; set; }
    
    /// <summary>
    /// Statement of a track
    /// </summary>
    public int Offset { get; set; }
    
    /// <summary>
    /// 20 next tracks after the actual to generate 20 tracks at the last track
    /// <remarks>Spotify don't give this key/value, it's a calculation</remarks>
    /// </summary>
    public int? NextOffset { get; set; }
    
    /// <summary>
    /// List of tracks in the playlist
    /// <remarks>Contains the album and the artist of a track</remarks>
    /// </summary>
    public List<SpotifyTrack> Tracks { get; set; }
}