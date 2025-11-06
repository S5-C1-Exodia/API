namespace API.DTO;

/// <summary>
/// Represents an image associated with a Spotify playlist, including its URL and dimensions.
/// </summary>
public sealed class SpotifyImage
{
    /// <summary>
    /// Gets or sets the URL of the image.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the height of the image in pixels.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the width of the image in pixels.
    /// </summary>
    public int? Width { get; set; }
}