using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.DAO;

/// <summary>
/// Data Access Object for managing playlist selection entries in the database.
/// Provides methods to delete playlist selection entries by session identifier.
/// </summary>
public class PlaylistSelectionDao(ISqlConnectionFactory factory) : IPlaylistSelectionDao
{
    private readonly ISqlConnectionFactory _factory = factory;

    /// <summary>
    /// Deletes all playlist selection entries associated with the specified session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier whose playlist selection entries should be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task DeleteBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = "delete from playlistselection where SessionId = @sid";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Deletes all playlist selection entries associated with the specified session identifier,
    /// using an existing MySQL connection and transaction.
    /// </summary>
    /// <param name="sessionId">The session identifier whose playlist selection entries should be deleted.</param>
    /// <param name="conn">An open <see cref="MySqlConnection"/> to use for the operation.</param>
    /// <param name="tx">An active <see cref="MySqlTransaction"/> to use for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="conn"/> or <paramref name="tx"/> is null.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task DeleteBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from playlistselection where SessionId = @sid";

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }
}