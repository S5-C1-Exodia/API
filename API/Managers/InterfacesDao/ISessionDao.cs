using Api.Models;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for session data access operations.
/// </summary>
public interface ISessionDao
{
    /// <summary>
    /// Inserts a new session.
    /// </summary>
    /// <param name="session">The session to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InsertAsync(AppSession session);

    /// <summary>
    /// Retrieves a session by its identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the <see cref="AppSession"/> if found; otherwise, null.
    /// </returns>
    Task<AppSession> GetAsync(string sessionId);
}