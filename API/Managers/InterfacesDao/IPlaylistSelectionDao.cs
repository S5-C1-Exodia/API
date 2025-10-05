using MySqlConnector;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for playlist selection data access operations.
/// </summary>
public interface IPlaylistSelectionDao
{
    /// <summary>
    /// Deletes all playlist selections for a given session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    Task DeleteBySessionAsync(string sessionId);

    /// <summary>
    /// Deletes all playlist selections for a given session within a transaction asynchronously.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="conn">The MySQL connection.</param>
    /// <param name="tx">The MySQL transaction.</param>
    Task DeleteBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);
}