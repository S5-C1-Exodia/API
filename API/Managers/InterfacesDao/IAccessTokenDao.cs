using MySqlConnector;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for data access operations related to access tokens.
/// Provides methods for deleting, retrieving, and upserting access tokens associated with a session identifier.
/// </summary>
public interface IAccessTokenDao
{
    /// <summary>
    /// Asynchronously deletes all access tokens associated with the specified session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier whose access tokens should be deleted. Must not be null or empty.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    Task DeleteBySessionAsync(string sessionId);

    /// <summary>
    /// Asynchronously deletes all access tokens associated with the specified session identifier, using the provided MySQL connection and transaction.
    /// </summary>
    /// <param name="sessionId">The session identifier whose access tokens should be deleted. Must not be null or empty.</param>
    /// <param name="conn">The <see cref="MySqlConnection"/> to use for the operation. Must be open and valid.</param>
    /// <param name="tx">The <see cref="MySqlTransaction"/> to use for the operation. Must be valid and associated with <paramref name="conn"/>.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="conn"/> or <paramref name="tx"/> is null.</exception>
    Task DeleteBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);

    /// <summary>
    /// Asynchronously retrieves a valid access token for the specified session, or null if none exists or is expired.
    /// </summary>
    /// <param name="sessionId">The session identifier to search for a valid access token.</param>
    /// <param name="nowUtc">The current UTC time used to check token validity.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the valid access token as a string, or null if not found or expired.
    /// </returns>
    Task<string?> GetValidBySessionAsync(string sessionId, DateTime nowUtc, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously upserts (inserts or updates) the access token and its expiration for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier for which to upsert the access token.</param>
    /// <param name="accessToken">The access token to store.</param>
    /// <param name="expiresAtUtc">The UTC expiration time of the access token.</param>
    /// <param name="nowUtc">The current UTC time.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous upsert operation.
    /// </returns>
    Task UpsertAsync(string sessionId, string accessToken, DateTime expiresAtUtc, DateTime nowUtc,
        CancellationToken ct = default);
}