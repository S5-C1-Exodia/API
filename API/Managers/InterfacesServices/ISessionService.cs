using System.Data.Common;
using Api.Models;

namespace Api.Managers.InterfacesServices
{
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
        /// <param name="createdAtUtc">The current UTC timestamp when the session is created.</param>
        /// <param name="expiresAtUtc">The UTC expiration timestamp for the session.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> whose result is the new session ID.
        /// </returns>
        Task<string> CreateSessionAsync(string deviceInfo, DateTime createdAtUtc, DateTime expiresAtUtc);

        /// <summary>
        /// Asynchronously retrieves a session by its identifier.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to retrieve.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> whose result is the <see cref="AppSession"/> if found; otherwise, null.
        /// </returns>
        Task<AppSession?> GetSessionAsync(string sessionId);

        /// <summary>
        /// Asynchronously deletes a session by its identifier using the provided connection and transaction.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to delete.</param>
        /// <param name="conn">The <see cref="DbConnection"/> to use for the operation.</param>
        /// <param name="tx">The <see cref="DbTransaction"/> to use for the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous delete operation.</returns>
        Task DeleteAsync(string sessionId, DbConnection conn, DbTransaction tx);

        /// <summary>
        /// Asynchronously deletes a session by its identifier (self-managed connection).
        /// Useful outside of a global transaction.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous delete operation.</returns>
        Task DeleteAsync(string sessionId);
    }
}