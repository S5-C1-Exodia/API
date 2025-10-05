using MySqlConnector;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Defines data access operations for access tokens, including deletion by session identifier.
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
}