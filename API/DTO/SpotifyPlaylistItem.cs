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
    /// Limit of track imported
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Statement of a track
    /// </summary>
    public int? Offset { get; set; }

    /// <summary>
    /// 20 next tracks after the actual to generate 20 tracks at the last track
    /// <remarks>Spotify don't give this key/value, it's a calculation</remarks>
    /// </summary>
    public int? NextOffset { get; set; }

    /// <summary>
    /// Gets or sets the image of the playlist.
    /// </summary>
    public SpotifyImage[]? Images { get; set; }

    /// <summary>
    /// Gets or sets the owner of the playlist.
    /// </summary>
    public SpotifyOwner? Owner { get; set; }

    /// <summary>
    /// Gets or sets tracks information of the playlist.
    /// </summary>
    public List<SpotifyTrack?> Tracks { get; set; }

    /// <summary>
    /// Tracks number in a playlist
    /// </summary>
    public SpotifyTracks TracksNumber { get; set; }
}