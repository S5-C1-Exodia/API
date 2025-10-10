namespace API.DTO;

/// <summary>
/// Represents the response from Spotify's playlists endpoint.
/// Contains metadata about the playlists and an array of playlist items.
/// </summary>
public sealed class SpotifyPlaylistsResponse
{
    /// <summary>
    /// The API endpoint URL for the playlists.
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// The maximum number of items returned.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// The URL to the next page of results, if any.
    /// </summary>
    public string? Next { get; set; }

    /// <summary>
    /// The offset of the items returned.
    /// </summary>
    public int? Offset { get; set; }

    /// <summary>
    /// The URL to the previous page of results, if any.
    /// </summary>
    public string? Previous { get; set; }

    /// <summary>
    /// The total number of playlists available.
    /// </summary>
    public int? Total { get; set; }

    /// <summary>
    /// The array of playlist items.
    /// </summary>
    public List<SpotifyPlaylistItem> Items { get; set; } = [];
}