using API.Managers.InterfacesServices;

namespace API.Services;

/// <summary>
/// Service responsible for managing configuration values related to Spotify and application settings.
/// Provides methods to retrieve various configuration parameters such as Spotify API endpoints, 
/// client credentials, and TTL (Time-To-Live) values for sessions and caches.
/// </summary>
public class ConfigService : IConfigService
{
    private readonly string _spotifyBaseUrl;
    private readonly string _spotifyClientId;
    private readonly string _spotifyRedirectUri;
    private readonly string _spotifyAuthorizeEndpoint;
    private readonly string _spotifyTokenEndpoint;
    private readonly int _spotifyPlaylistPageSize;
    private readonly int _spotifyCacheTtlMinutes;
    private readonly string _deeplinkSchemeHost;
    private readonly int _pkceTtlMinutes;
    private readonly int _sessionTtlMinutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class with the specified configuration values.
    /// </summary>
    /// <param name="spotifyBaseUrl">The base URL for the Spotify API.</param>
    /// <param name="spotifyClientId">The client ID for the Spotify application.</param>
    /// <param name="spotifyRedirectUri">The redirect URI for Spotify authentication.</param>
    /// <param name="spotifyAuthorizeEndpoint">The endpoint for Spotify authorization.</param>
    /// <param name="spotifyTokenEndpoint">The endpoint for Spotify token exchange.</param>
    /// <param name="spotifyPlaylistPageSize">The maximum number of playlists per page (1-50).</param>
    /// <param name="spotifyCacheTtlMinutes">The cache TTL (Time-To-Live) in minutes for Spotify playlists.</param>
    /// <param name="deeplinkSchemeHost">The host for the deeplink scheme.</param>
    /// <param name="pkceTtlMinutes">The TTL in minutes for PKCE (Proof Key for Code Exchange) codes.</param>
    /// <param name="sessionTtlMinutes">The TTL in minutes for user sessions.</param>
    /// <exception cref="ArgumentException">Thrown if any required parameter is null, empty, or invalid.</exception>
    public ConfigService(
        string spotifyBaseUrl,
        string spotifyClientId,
        string spotifyRedirectUri,
        string spotifyAuthorizeEndpoint,
        string spotifyTokenEndpoint,
        int spotifyPlaylistPageSize,
        int spotifyCacheTtlMinutes,
        string deeplinkSchemeHost,
        int pkceTtlMinutes,
        int sessionTtlMinutes)
    {
        if (string.IsNullOrWhiteSpace(spotifyClientId))
            throw new ArgumentException("spotifyClientId cannot be null or empty.", nameof(spotifyClientId));

        if (string.IsNullOrWhiteSpace(spotifyRedirectUri))
            throw new ArgumentException("spotifyRedirectUri cannot be null or empty.", nameof(spotifyRedirectUri));

        if (string.IsNullOrWhiteSpace(spotifyAuthorizeEndpoint))
            throw new ArgumentException("spotifyAuthorizeEndpoint cannot be null or empty.", nameof(spotifyAuthorizeEndpoint));

        if (string.IsNullOrWhiteSpace(spotifyTokenEndpoint))
            throw new ArgumentException("spotifyTokenEndpoint cannot be null or empty.", nameof(spotifyTokenEndpoint));

        if (string.IsNullOrWhiteSpace(deeplinkSchemeHost))
            throw new ArgumentException("deeplinkSchemeHost cannot be null or empty.", nameof(deeplinkSchemeHost));

        if (pkceTtlMinutes <= 0)
            throw new ArgumentException("pkceTtlMinutes must be positive.", nameof(pkceTtlMinutes));

        if (sessionTtlMinutes <= 0)
            throw new ArgumentException("sessionTtlMinutes must be positive.", nameof(sessionTtlMinutes));

        if (spotifyPlaylistPageSize <= 0 || spotifyPlaylistPageSize > 50)
            throw new ArgumentException("spotifyPlaylistPageSize must be between 1 and 50.", nameof(spotifyPlaylistPageSize));

        if (spotifyCacheTtlMinutes <= 0)
            throw new ArgumentException("spotifyCacheTtlMinutes must be positive.", nameof(spotifyCacheTtlMinutes));

        _spotifyBaseUrl = spotifyBaseUrl;
        _spotifyPlaylistPageSize = spotifyPlaylistPageSize;
        _spotifyCacheTtlMinutes = spotifyCacheTtlMinutes;
        _spotifyClientId = spotifyClientId;
        _spotifyRedirectUri = spotifyRedirectUri;
        _spotifyAuthorizeEndpoint = spotifyAuthorizeEndpoint;
        _spotifyTokenEndpoint = spotifyTokenEndpoint;
        _deeplinkSchemeHost = deeplinkSchemeHost;
        _pkceTtlMinutes = pkceTtlMinutes;
        _sessionTtlMinutes = sessionTtlMinutes;
    }


    public string GetSpotifyClientId() => _spotifyClientId;
    public string GetSpotifyRedirectUri() => _spotifyRedirectUri;
    public string GetSpotifyAuthorizeEndpoint() => _spotifyAuthorizeEndpoint;
    public string GetSpotifyTokenEndpoint() => _spotifyTokenEndpoint;
    public string GetDeeplinkSchemeHost() => _deeplinkSchemeHost;
    public int GetPkceTtlMinutes() => _pkceTtlMinutes;
    public int GetSessionTtlMinutes() => _sessionTtlMinutes;
    public string GetSpotifyApiBaseUrl() => _spotifyBaseUrl;
    public int GetSpotifyPlaylistsPageSize() => _spotifyPlaylistPageSize;
    public int GetPlaylistCacheTtlMinutes() => _spotifyCacheTtlMinutes;
}