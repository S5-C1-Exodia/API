using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.DAO;

/// <summary>
/// Data Access Object for managing playlist selection entries in the database.
/// Provides methods to delete playlist selection entries by session identifier.
/// <param name=" _factory">The SQL connection factory for database connections.</param>
/// </summary>
public class PlaylistSelectionDao(ISqlConnectionFactory _factory) : IPlaylistSelectionDao
{
    /// <inheritdoc />
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

    /// <inheritdoc />
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

    public async Task BulkInsertAsync(
        string sessionId,
        string provider,
        string providerUserId,
        IEnumerable<string> playlistIds,
        DateTime createdAtUtc,
        MySqlConnection conn,
        MySqlTransaction tx)
    {
        const string sql = @"
insert into playlistselection (SessionId, Provider, ProviderUserId, PlaylistId, CreatedAt)
values (@sid, @prov, @puid, @pid, @ts);";

        foreach (string pid in playlistIds.Distinct())
        {
            using MySqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@sid", sessionId);
            cmd.Parameters.AddWithValue("@prov", provider);
            cmd.Parameters.AddWithValue("@puid", providerUserId);
            cmd.Parameters.AddWithValue("@pid", pid);
            cmd.Parameters.AddWithValue("@ts", createdAtUtc);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task<int> BulkInsertIfNotExistsAsync(
        string sessionId,
        string provider,
        string providerUserId,
        IEnumerable<string> playlistIds,
        DateTime createdAtUtc,
        MySqlConnection conn,
        MySqlTransaction tx)
    {
        const string sql = @"
insert ignore into playlistselection (SessionId, Provider, ProviderUserId, PlaylistId, CreatedAt)
values (@sid, @prov, @puid, @pid, @ts);";

        int inserted = 0;
        foreach (string pid in playlistIds.Distinct())
        {
            using MySqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@sid", sessionId);
            cmd.Parameters.AddWithValue("@prov", provider);
            cmd.Parameters.AddWithValue("@puid", providerUserId);
            cmd.Parameters.AddWithValue("@pid", pid);
            cmd.Parameters.AddWithValue("@ts", createdAtUtc);
            int rows = await cmd.ExecuteNonQueryAsync();
            if (rows > 0) inserted += rows;
        }

        return inserted;
    }

    public async Task<int> BulkDeleteByIdsAsync(
        string sessionId,
        IEnumerable<string> playlistIds,
        MySqlConnection conn,
        MySqlTransaction tx)
    {
        const string sql = @"
delete from playlistselection
where SessionId = @sid and PlaylistId = @pid;";

        int deleted = 0;
        foreach (string pid in playlistIds.Distinct())
        {
            using MySqlCommand cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@sid", sessionId);
            cmd.Parameters.AddWithValue("@pid", pid);
            int rows = await cmd.ExecuteNonQueryAsync();
            if (rows > 0) deleted += rows;
        }

        return deleted;
    }

    public async Task<IReadOnlyList<string>> GetIdsBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        await using MySqlConnection conn = _factory.Create();
        await conn.OpenAsync();

        const string sql = @"
select PlaylistId
from playlistselection
where SessionId = @sid
order by PlaylistId;";

        await using MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);

        List<string> result = new List<string>();
        await using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }
}