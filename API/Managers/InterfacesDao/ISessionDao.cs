using Api.Models;
using MySqlConnector;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for session data access operations.
/// Provides methods to insert, retrieve, and delete session records in the data store.
/// </summary>
public interface ISessionDao
{
    /// <summary>
    /// Asynchronously inserts a new session into the data store.
    /// </summary>
    /// <param name="session">The <see cref="AppSession"/> object to insert.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous insert operation.
    /// </returns>
    Task InsertAsync(AppSession session);

    /// <summary>
    /// Asynchronously retrieves a session by its identifier.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to retrieve.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains the <see cref="AppSession"/> if found; otherwise, null.
    /// </returns>
    Task<AppSession?> GetAsync(string? sessionId);

    /// <summary>
    /// Asynchronously deletes a session by its identifier.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to delete.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous delete operation.
    /// </returns>
    Task DeleteAsync(string sessionId);

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
}