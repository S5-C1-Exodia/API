using API.DTO;

namespace Api.Managers.InterfacesHelpers;

/// <summary>
/// Interface providing helper methods to interact with Spotify's OAuth endpoints.
/// </summary>
public interface ISpotifyOAuthHelper
{
    /// <summary>
    /// Exchanges an authorization code for access and refresh tokens.
    /// </summary>
    /// <param name="code">The authorization code received from Spotify after user authentication.</param>
    /// <param name="redirectUri">The redirect URI used during the OAuth flow, must match the one registered with Spotify.</param>
    /// <param name="codeVerifier">The PKCE code verifier used to secure the authorization code exchange.</param>
    /// <returns>
    /// A <see cref="TokenInfo"/> object containing the access token, refresh token, expiration time, scope, and provider user ID.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if any argument is invalid (e.g., null or empty).</exception>
    /// <exception cref="Exception">Thrown if the token exchange with Spotify fails.</exception>
    Task<TokenInfo> ExchangeCodeForTokensAsync(string code, string redirectUri, string codeVerifier);

    /// <summary>
    /// Refreshes an access token using the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token previously obtained from Spotify.</param>
    /// <param name="ct">Optional cancellation token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="RefreshResult"/> containing the new access token, its expiry, and optionally a rotated refresh token.
    /// </returns>
    Task<RefreshResult> RefreshTokensAsync(string refreshToken, CancellationToken ct = default);
}