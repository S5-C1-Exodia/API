namespace API.Managers.InterfacesServices;

public interface IConfigService
{
    string GetSpotifyClientId();
    string GetSpotifyRedirectUri();
    string GetSpotifyAuthorizeEndpoint();
    string GetSpotifyTokenEndpoint();
    string GetDeeplinkSchemeHost();
    int GetPkceTtlMinutes();
    int GetSessionTtlMinutes();
}