namespace API.Managers.InterfacesServices;

    /// <summary>
    /// Provides configuration values for the application.
    /// </summary>
public interface IConfigService
{
    /// <summary>
    /// Gets the Spotify client ID.
    /// </summary>
    /// <returns>The Spotify client ID.</returns>
    string GetSpotifyClientId();

    /// <summary>
    /// Gets the Spotify redirect URI.
    /// </summary>
    /// <returns>The Spotify redirect URI.</returns>
    string GetSpotifyRedirectUri();

    /// <summary>
    /// Gets the Spotify authorization endpoint URL.
    /// </summary>
    /// <returns>The Spotify authorization endpoint URL.</returns>
    string GetSpotifyAuthorizeEndpoint();

    /// <summary>
    /// Gets the Spotify token endpoint URL.
    /// </summary>
    /// <returns>The Spotify token endpoint URL.</returns>
    string GetSpotifyTokenEndpoint();

    /// <summary>
    /// Gets the deeplink scheme and host for the mobile app.
    /// </summary>
    /// <returns>The deeplink scheme and host.</returns>
    string GetDeeplinkSchemeHost();

    /// <summary>
    /// Gets the PKCE time-to-live in minutes.
    /// </summary>
    /// <returns>The PKCE TTL in minutes.</returns>
    int GetPkceTtlMinutes();

    /// <summary>
    /// Gets the session time-to-live in minutes.
    /// </summary>
    /// <returns>The session TTL in minutes.</returns>
    int GetSessionTtlMinutes();
}