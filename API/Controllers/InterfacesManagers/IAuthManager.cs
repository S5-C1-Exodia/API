using API.DTO;

namespace API.Controllers.InterfacesManagers;

/// <summary>
/// Interface for authentication management logic.
/// </summary>
public interface IAuthManager
{
    /// <summary>
    /// Starts the authentication process for the specified scopes.
    /// </summary>
    /// <param name="scopes">The list of scopes requested for authentication.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="AuthStartResponseDto"/>
    /// with the authorization URL and state.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
    Task<AuthStartResponseDto> StartAuthAsync(IList<string> scopes);

    /// <summary>
    /// Handles the OAuth callback, exchanges the code for tokens, and creates a session.
    /// </summary>
    /// <param name="code">The authorization code returned by the OAuth provider.</param>
    /// <param name="state">The PKCE state parameter.</param>
    /// <param name="deviceInfo">Optional device information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the deeplink URL to redirect to.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if arguments are invalid.</exception>
    /// <exception cref="API.Errors.InvalidStateException">Thrown if the state is invalid or expired.</exception>
    /// <exception cref="API.Errors.TokenExchangeFailedException">Thrown if the token exchange fails.</exception>
    Task<string> HandleCallbackAsync(string code, string state, string? deviceInfo);

    /// <summary>
    /// Logs out a user by purging session and related data, denylisting the refresh token, and auditing the operation.
    /// </summary>
    /// <param name="sessionId">The session identifier to log out.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    Task LogoutAsync(string sessionId);

}