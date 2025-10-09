namespace API.DTO;

/// <summary>
/// Represents an album
/// </summary>
public class AlbumDTO
{
    /// <summary>
    /// ID of the current album
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// URL to the image
    /// <remarks>Spotify gives a several images url</remarks>
    /// </summary>
    public List<SpotifyImage> Images { get; set; }
}