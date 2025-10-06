namespace API.DTO;

/// <summary>
/// Internal model representing a single playlist item from Spotify.
/// </summary>
public sealed class SpotifyPlaylistItem
{
    /// <summary>
    /// Gets or sets the unique identifier of the playlist.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///  Gets or sets the name of the playlist.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the image of the playlist.
    /// </summary>
    public SpotifyImage[]? Images { get; set; }

    /// <summary>
    /// Gets or sets the owner of the playlist.
    /// </summary>
    public SpotifyOwner? Owner { get; set; }

    /// <summary>
    /// Gets or sets the tracks information of the playlist.
    /// </summary>s
    public SpotifyTracks? Tracks { get; set; }
}