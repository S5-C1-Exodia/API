namespace API.DTO;

/// <summary>
/// Representing a track
/// </summary>
public class SpotifyTrack
{
    /// <summary>
    /// Track ID
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Track Name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Author of the track
    /// </summary>
    public List<ArtistDTO> Artists { get; set; }
    
    /// <summary>
    /// Album of the track
    /// </summary>
    public AlbumDTO Album { get; set; }
    
}