using API.DTO;

namespace Api.Managers.InterfacesHelpers;

/// <summary>
/// Provides methods to interact with Spotify's OAuth endpoints.
/// </summary>
public interface ISpotifyOAuthHelper
{
    /// <summary>
    /// Exchanges an authorization code for access and refresh tokens.
    /// </summary>
    /// <param name="code">The authorization code received from Spotify.</param>
    /// <param name="redirectUri">The redirect URI used in the OAuth flow.</param>
    /// <param name="codeVerifier">The PKCE code verifier.</param>
    /// <returns>
    /// A <see cref="TokenInfo"/> containing access and refresh tokens, expiration, scope, and provider user ID.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if any argument is invalid.</exception>
    /// <exception cref="Exception">Thrown if the token exchange fails.</exception>
    Task<TokenInfo> ExchangeCodeForTokensAsync(string code, string redirectUri, string codeVerifier);
}