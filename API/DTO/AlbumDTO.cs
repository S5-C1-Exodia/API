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
    /// </summary>
    public string ImageUrl { get; set; }
}