namespace API.DTO;

/// <summary>
/// Spotify web API response for /playlists/playlist_id/tracks
/// </summary>
public class PlaylistTracksResponse
{
    /// <summary>
    /// List of tracks json
    /// </summary>
    public List<SpotifyTrackItem> Items { get; set; }
    
    /// <summary>
    /// Tracks limit to get
    /// </summary>
    public int Limit { get; set; }
    
    /// <summary>
    /// Index of a track
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Constructor
    /// <example>Limit set to 20 and offset is set to 0 as default value</example>
    /// </summary>
    public PlaylistTracksResponse()
    {
        Limit = 20;
        Offset = 0;
    }
}