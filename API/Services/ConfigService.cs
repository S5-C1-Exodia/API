using API.Managers.InterfacesServices;

namespace API.Services;

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

    /// <inheritdoc />
    public string GetSpotifyClientId() => _spotifyClientId;
    
    /// <inheritdoc />
    public string GetSpotifyRedirectUri() => _spotifyRedirectUri;
    
    /// <inheritdoc />
    public string GetSpotifyAuthorizeEndpoint() => _spotifyAuthorizeEndpoint;
    
    /// <inheritdoc />
    public string GetSpotifyTokenEndpoint() => _spotifyTokenEndpoint;
    
    /// <inheritdoc />
    public string GetDeeplinkSchemeHost() => _deeplinkSchemeHost;
    
    /// <inheritdoc />
    public int GetPkceTtlMinutes() => _pkceTtlMinutes;
    
    /// <inheritdoc />
    public int GetSessionTtlMinutes() => _sessionTtlMinutes;
    
    /// <inheritdoc />
    public string GetSpotifyApiBaseUrl() => _spotifyBaseUrl;
    
    /// <inheritdoc />
    public int GetSpotifyPlaylistsPageSize() => _spotifyPlaylistPageSize;
    
    /// <inheritdoc />
    public int GetPlaylistCacheTtlMinutes() => _spotifyCacheTtlMinutes;
}