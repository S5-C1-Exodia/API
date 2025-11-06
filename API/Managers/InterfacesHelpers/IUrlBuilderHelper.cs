namespace Api.Managers.InterfacesHelpers;

/// <summary>
/// Provides methods to build OAuth authorization URLs.
/// </summary>
public interface IUrlBuilderHelper
{
    /// <summary>
    /// Builds the Spotify authorization URL for the OAuth flow.
    /// </summary>
    /// <param name="clientId">The Spotify client ID.</param>
    /// <param name="redirectUri">The redirect URI.</param>
    /// <param name="scopes">The requested scopes.</param>
    /// <param name="state">The state parameter for CSRF protection.</param>
    /// <param name="codeChallenge">The PKCE code challenge.</param>
    /// <param name="codeChallengeMethod">The PKCE code challenge method (e.g., "S256").</param>
    /// <returns>The complete authorization URL as a string.</returns>
    string BuildAuthorizeUrl(string clientId, string redirectUri, string[] scopes, string state, string codeChallenge,
        string codeChallengeMethod);
}