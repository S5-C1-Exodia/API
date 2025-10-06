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

        await using var conn = factory.Create();
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

        await using var conn = factory.Create();
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

    /// <inheritdoc />
    public async Task<string?> GetPageJsonAsync(string sessionId, string? pageToken, DateTime nowUtc,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = @"
select pc.json
from playlistcache pc
join playlistcache_session pcs
  on pcs.provider_user_id = pc.provider_user_id
 and (
       (pcs.page_token is null and pc.page_token is null)
       or (pcs.page_token = pc.page_token)
     )
where pcs.session_id = @sid
  and (
       (pc.page_token is null and @ptok is null)
       or (pc.page_token = @ptok)
      )
  and pc.expires_at > @now
limit 1;";

        await using var conn = factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        cmd.Parameters.AddWithValue("@ptok", (object?)pageToken ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@now", nowUtc);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is DBNull or null ? null : (string)result;
    }

    // =======================
    // NEW: Upsert page + link
    // =======================
    /// <inheritdoc />
    public async Task UpsertPageAsync(
        string sessionId,
        string providerUserId,
        string? pageToken,
        string pageJson,
        DateTime expiresAtUtc,
        DateTime nowUtc,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));
        if (string.IsNullOrWhiteSpace(pageJson))
            throw new ArgumentException("pageJson cannot be null or empty.", nameof(pageJson));

        const string upsertCache = @"
insert into playlistcache (ProviderUserId, PageToken, Json, UpdatedAt, ExpiresAt)
values (@puid, @ptok, @json, @now, @exp)
on duplicate key update
  Json = values(Json),
  UpdatedAt = values(UpdatedAt),
  ExpiresAt = values(ExpiresAt);";

        const string upsertLink = @"
insert into playlistcache_session (SessionId, ProviderUserId, PageToken, LinkedAt)
values (@sid, @puid, @ptok, @now)
on duplicate key update
  LinkedAt = values(LinkedAt);";

        await using var conn = factory.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        // Upsert cache row
        await using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = (MySqlTransaction)tx;
            cmd.CommandText = upsertCache;
            cmd.Parameters.AddWithValue("@puid", providerUserId);
            cmd.Parameters.AddWithValue("@ptok", (object?)pageToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@json", pageJson);
            cmd.Parameters.AddWithValue("@now", nowUtc);
            cmd.Parameters.AddWithValue("@exp", expiresAtUtc);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // Upsert session link
        await using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = (MySqlTransaction)tx;
            cmd.CommandText = upsertLink;
            cmd.Parameters.AddWithValue("@sid", sessionId);
            cmd.Parameters.AddWithValue("@puid", providerUserId);
            cmd.Parameters.AddWithValue("@ptok", (object?)pageToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@now", nowUtc);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
    }
}