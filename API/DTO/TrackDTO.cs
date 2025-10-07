namespace API.DTO;

/// <summary>
/// Representing a track
/// </summary>
public class TrackDTO
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
    /// Image for the track
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Author of the track
    /// </summary>
    public ArtistDTO Author { get; set; }
    
    /// <summary>
    /// Album of the track
    /// </summary>
    public AlbumDTO Album { get; set; }
}