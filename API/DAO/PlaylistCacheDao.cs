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
    
    /// <inheritdoc />
    public async Task DeleteByProviderUserAsync(string providerUserId)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));

        const string sql = "delete from playlistcache where ProviderUserId = @puid";

        await using MySqlConnection conn = factory.Create();
        await conn.OpenAsync();
        await using MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@puid", providerUserId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <inheritdoc />
    public async Task DeleteByProviderUserAsync(string providerUserId, MySqlConnection conn, MySqlTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from playlistcache where ProviderUserId = @puid";

        await using MySqlCommand cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@puid", providerUserId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <inheritdoc />
    public async Task DeleteLinksBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = "delete from playlistcache_session where SessionId = @sid";

        await using MySqlConnection conn = factory.Create();
        await conn.OpenAsync();
        await using MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <inheritdoc />
    public async Task DeleteLinksBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from playlistcache_session where SessionId = @sid";

        await using MySqlCommand cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <inheritdoc />
    public async Task<string?> GetPageJsonAsync(string sessionId, string pageToken, DateTime nowUtc,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        
        const string sql = @"
select pc.Json
from playlistcache pc
join playlistcache_session pcs
  on pcs.ProviderUserId = pc.ProviderUserId
 and (
       (pcs.PageToken is null and pc.PageToken is null)
       or (pcs.PageToken = pc.PageToken)
     )
where pcs.SessionId = @sid
  and (
       (pc.PageToken is null and @ptok is null)
       or (pc.PageToken = @ptok)
      )
  and pc.ExpiresAt > @now
limit 1;";

        await using MySqlConnection conn = factory.Create();
        await conn.OpenAsync(ct);

        await using MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        cmd.Parameters.AddWithValue("@ptok", (object?)pageToken ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@now", nowUtc);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is DBNull or null ? null : (string)result;
    }
    
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

        await using MySqlConnection conn = factory.Create();
        await conn.OpenAsync(ct);
        await using MySqlTransaction tx = await conn.BeginTransactionAsync(ct);

        // Upsert cache row
        await using (MySqlCommand cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = upsertCache;
            cmd.Parameters.AddWithValue("@puid", providerUserId);
            cmd.Parameters.AddWithValue("@ptok", (object?)pageToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@json", pageJson);
            cmd.Parameters.AddWithValue("@now", nowUtc);
            cmd.Parameters.AddWithValue("@exp", expiresAtUtc);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // Upsert session link
        await using (MySqlCommand cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
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