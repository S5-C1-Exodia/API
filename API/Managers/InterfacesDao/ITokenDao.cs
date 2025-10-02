namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for token data access operations.
/// </summary>
public interface ITokenDao
{
    /// <summary>
    /// Saves a token set by PKCE state.
    /// </summary>
    /// <param name="state">The PKCE state value.</param>
    /// <param name="provider">The OAuth provider name.</param>
    /// <param name="providerUserId">The user ID at the provider.</param>
    /// <param name="refreshToken">The refresh token string.</param>
    /// <param name="scope">The OAuth scopes.</param>
    /// <param name="accessExpiresAt">The UTC expiration date and time of the access token.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the new token set ID.
    /// </returns>
    Task<long> SaveByStateAsync(string state, string provider, string providerUserId,
        string refreshToken, string scope, DateTime accessExpiresAt);

    /// <summary>
    /// Attaches a token set to a session.
    /// </summary>
    /// <param name="tokenSetId">The token set identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AttachToSessionAsync(long tokenSetId, string sessionId);
}