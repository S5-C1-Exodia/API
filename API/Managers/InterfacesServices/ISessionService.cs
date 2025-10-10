using Api.Models;
using MySqlConnector;

namespace Api.Managers.InterfacesServices;

/// <summary>
/// Interface for session management services.
/// Provides methods to create, retrieve, and delete application sessions.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Asynchronously creates a new session.
    /// </summary>
    /// <param name="deviceInfo">Information about the device initiating the session.</param>
    /// <param name="nowUtc">The current UTC date and time.</param>
    /// <param name="expiresAt">The UTC expiration date and time for the session.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains the new session ID as a string.
    /// </returns>
    Task<string> CreateSessionAsync(string deviceInfo, DateTime nowUtc, DateTime expiresAt);

    /// <summary>
    /// Asynchronously retrieves a session by its identifier.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to retrieve.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains the <see cref="AppSession"/> if found; otherwise, null.
    /// </returns>
    Task<AppSession?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Asynchronously deletes a session by its identifier using the provided MySQL connection and transaction.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to delete.</param>
    /// <param name="conn">The <see cref="MySqlConnection"/> to use for the operation.</param>
    /// <param name="tx">The <see cref="MySqlTransaction"/> to use for the operation.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous delete operation.
    /// </returns>
    Task DeleteAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);

    /// <summary>
    /// Asynchronously deletes a session by its identifier, opening its own connection.
    /// Useful outside of global transactions.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to delete.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous delete operation.
    /// </returns>
    Task DeleteAsync(string sessionId);
}