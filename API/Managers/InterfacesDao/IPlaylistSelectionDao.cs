using System.Data.Common;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for managing playlist selection data access operations.
/// Provides methods for inserting, deleting, and retrieving playlist selections
/// associated with a specific session.
/// </summary>
public interface IPlaylistSelectionDao
{
    /// <summary>
    /// Deletes all playlist selections for a given session asynchronously.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteBySessionAsync(string sessionId);

    /// <summary>
    /// Deletes all playlist selections for a given session within a transaction asynchronously.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="conn">The database connection to use for the operation.</param>
    /// <param name="tx">The database transaction to use for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteBySessionAsync(string sessionId, DbConnection conn, DbTransaction tx);

    /// <summary>
    /// Inserts multiple playlist selections in bulk for a given session asynchronously.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="provider">The name of the provider (e.g., Spotify).</param>
    /// <param name="providerUserId">The unique identifier of the user in the provider system.</param>
    /// <param name="playlistIds">A collection of playlist IDs to insert.</param>
    /// <param name="createdAtUtc">The UTC timestamp when the playlists were created.</param>
    /// <param name="conn">The database connection to use for the operation.</param>
    /// <param name="tx">The database transaction to use for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BulkInsertAsync(
        string sessionId,
        string provider,
        string providerUserId,
        IEnumerable<string> playlistIds,
        DateTime createdAtUtc,
        DbConnection conn,
        DbTransaction tx);

    /// <summary>
    /// Inserts multiple playlist selections in bulk if they do not already exist, asynchronously.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="provider">The name of the provider (e.g., Spotify).</param>
    /// <param name="providerUserId">The unique identifier of the user in the provider system.</param>
    /// <param name="playlistIds">A collection of playlist IDs to insert.</param>
    /// <param name="createdAtUtc">The UTC timestamp when the playlists were created.</param>
    /// <param name="conn">The database connection to use for the operation.</param>
    /// <param name="tx">The database transaction to use for the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the number of rows inserted as the result.
    /// </returns>
    Task<int> BulkInsertIfNotExistsAsync(
        string sessionId,
        string provider,
        string providerUserId,
        IEnumerable<string> playlistIds,
        DateTime createdAtUtc,
        DbConnection conn,
        DbTransaction tx);

    /// <summary>
    /// Deletes multiple playlist selections by their IDs in bulk asynchronously.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="playlistIds">A collection of playlist IDs to delete.</param>
    /// <param name="conn">The database connection to use for the operation.</param>
    /// <param name="tx">The database transaction to use for the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the number of rows deleted as the result.
    /// </returns>
    Task<int> BulkDeleteByIdsAsync(
        string sessionId,
        IEnumerable<string> playlistIds,
        DbConnection conn,
        DbTransaction tx);

    /// <summary>
    /// Retrieves a list of playlist IDs associated with a given session asynchronously.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a read-only list of playlist IDs as the result.
    /// </returns>
    Task<IReadOnlyList<string>> GetIdsBySessionAsync(string sessionId);
}