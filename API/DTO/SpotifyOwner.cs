namespace API.DTO;

/// <summary>
/// Internal model representing the owner of a Spotify playlist.
/// </summary>
public sealed class SpotifyOwner
{
    /// <summary>
    /// Current ID of the user
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Pseudo of the current user
    /// </summary>
    public string? DisplayName { get; set; }
}
