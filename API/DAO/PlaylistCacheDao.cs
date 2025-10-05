using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.DAO;

/// <summary>
/// Data Access Object for managing playlist cache entries in the database.
/// Provides methods to delete playlist cache entries and their session links by provider user or session identifier.
/// </summary>
public class PlaylistCacheDao(ISqlConnectionFactory factory) : IPlaylistCacheDao
{
    private readonly ISqlConnectionFactory _factory = factory;

    /// <summary>
    /// Deletes all playlist cache entries associated with the specified provider user identifier.
    /// </summary>
    /// <param name="providerUserId">The provider user identifier whose playlist cache entries should be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="providerUserId"/> is null or empty.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task DeleteByProviderUserAsync(string providerUserId)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));

        const string sql = "delete from playlistcache where ProviderUserId = @puid";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@puid", providerUserId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Deletes all playlist cache entries associated with the specified provider user identifier,
    /// using an existing MySQL connection and transaction.
    /// </summary>
    /// <param name="providerUserId">The provider user identifier whose playlist cache entries should be deleted.</param>
    /// <param name="conn">An open <see cref="MySqlConnection"/> to use for the operation.</param>
    /// <param name="tx">An active <see cref="MySqlTransaction"/> to use for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="providerUserId"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="conn"/> or <paramref name="tx"/> is null.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task DeleteByProviderUserAsync(string providerUserId, MySqlConnection conn, MySqlTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from playlistcache where ProviderUserId = @puid";

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@puid", providerUserId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Deletes all playlist cache session links associated with the specified session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier whose playlist cache session links should be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task DeleteLinksBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = "delete from playlistcache_session where SessionId = @sid";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Deletes all playlist cache session links associated with the specified session identifier,
    /// using an existing MySQL connection and transaction.
    /// </summary>
    /// <param name="sessionId">The session identifier whose playlist cache session links should be deleted.</param>
    /// <param name="conn">An open <see cref="MySqlConnection"/> to use for the operation.</param>
    /// <param name="tx">An active <see cref="MySqlTransaction"/> to use for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="conn"/> or <paramref name="tx"/> is null.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task DeleteLinksBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from playlistcache_session where SessionId = @sid";

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }
}