using System.Data.Common;
using Api.Models;

namespace Api.Managers.InterfacesDao
{
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
        /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous insert operation.
        /// </returns>
        Task InsertAsync(AppSession session, CancellationToken ct = default);

        /// <summary>
        /// Asynchronously retrieves a session by its identifier.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to retrieve.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// The result contains the <see cref="AppSession"/> if found; otherwise, null.
        /// </returns>
        Task<AppSession?> GetAsync(string? sessionId, CancellationToken ct = default);

        /// <summary>
        /// Asynchronously deletes a session by its identifier.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to delete.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous delete operation.
        /// </returns>
        Task DeleteAsync(string sessionId, CancellationToken ct = default);

        /// <summary>
        /// Asynchronously deletes a session by its identifier using the provided database connection and transaction.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to delete.</param>
        /// <param name="conn">The <see cref="DbConnection"/> to use for the operation. Must be open and valid.</param>
        /// <param name="tx">The <see cref="DbTransaction"/> to use for the operation. Must be valid and associated with <paramref name="conn"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous delete operation.
        /// </returns>
        Task DeleteAsync(string sessionId, DbConnection conn, DbTransaction tx);
    }
}