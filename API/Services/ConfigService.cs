using API.Managers.InterfacesServices;

namespace API.Services;

public class ConfigService : IConfigService
{
    private readonly string _spotifyClientId;
    private readonly string _spotifyRedirectUri;
    private readonly string _spotifyAuthorizeEndpoint;
    private readonly string _spotifyTokenEndpoint;
    private readonly string _deeplinkSchemeHost;
    private readonly int _pkceTtlMinutes;
    private readonly int _sessionTtlMinutes;

    public ConfigService(
        string spotifyClientId,
        string spotifyRedirectUri,
        string spotifyAuthorizeEndpoint,
        string spotifyTokenEndpoint,
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

        _spotifyClientId = spotifyClientId;
        _spotifyRedirectUri = spotifyRedirectUri;
        _spotifyAuthorizeEndpoint = spotifyAuthorizeEndpoint;
        _spotifyTokenEndpoint = spotifyTokenEndpoint;
        _deeplinkSchemeHost = deeplinkSchemeHost;
        _pkceTtlMinutes = pkceTtlMinutes;
        _sessionTtlMinutes = sessionTtlMinutes;
    }

    public string GetSpotifyClientId()
    {
        return _spotifyClientId;
    }

    public string GetSpotifyRedirectUri()
    {
        return _spotifyRedirectUri;
    }

    public string GetSpotifyAuthorizeEndpoint()
    {
        return _spotifyAuthorizeEndpoint;
    }

    public string GetSpotifyTokenEndpoint()
    {
        return _spotifyTokenEndpoint;
    }

    public string GetDeeplinkSchemeHost()
    {
        return _deeplinkSchemeHost;
    }

    public int GetPkceTtlMinutes()
    {
        return _pkceTtlMinutes;
    }

    public int GetSessionTtlMinutes()
    {
        return _sessionTtlMinutes;
    }
}