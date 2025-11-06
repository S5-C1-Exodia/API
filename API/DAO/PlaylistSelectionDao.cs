using System.Data.Common;
using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.DAO;

/// <summary>
/// Data Access Object for managing playlist selection entries in the database.
/// Provides methods to delete playlist selection entries by session identifier.
/// </summary>
public class PlaylistSelectionDao(ISqlConnectionFactory factory) : IPlaylistSelectionDao
{
    /// <inheritdoc />
    public async Task DeleteBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = "delete from playlistselection where SessionId = @sid";

        var ct = CancellationToken.None;
        await using DbConnection conn = await factory.CreateOpenAsync(ct);
        await using DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var param = cmd.CreateParameter();
        param.ParameterName = "@sid";
        param.Value = sessionId;
        cmd.Parameters.Add(param);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteBySessionAsync(string sessionId, DbConnection conn, DbTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from playlistselection where SessionId = @sid";

        await using DbCommand cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;

        var param = cmd.CreateParameter();
        param.ParameterName = "@sid";
        param.Value = sessionId;
        cmd.Parameters.Add(param);

        await cmd.ExecuteNonQueryAsync(CancellationToken.None);
    }

    /// <summary>
    /// Asynchronously performs a bulk insert of playlist selection entries for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="provider">The provider name.</param>
    /// <param name="providerUserId">The provider user identifier.</param>
    /// <param name="playlistIds">The collection of playlist identifiers to insert.</param>
    /// <param name="createdAtUtc">The creation timestamp in UTC.</param>
    /// <param name="conn">The database connection.</param>
    /// <param name="tx">The database transaction.</param>
    public async Task BulkInsertAsync(
        string sessionId,
        string provider,
        string providerUserId,
        IEnumerable<string> playlistIds,
        DateTime createdAtUtc,
        DbConnection conn,
        DbTransaction tx)
    {
        const string sql = @"
        insert into playlistselection (SessionId, Provider, ProviderUserId, PlaylistId, CreatedAt)
        values (@sid, @prov, @puid, @pid, @ts);";

        foreach (string pid in playlistIds.Distinct())
        {
            await using DbCommand cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;

            var paramSid = cmd.CreateParameter();
            paramSid.ParameterName = "@sid";
            paramSid.Value = sessionId;
            cmd.Parameters.Add(paramSid);

            var paramProv = cmd.CreateParameter();
            paramProv.ParameterName = "@prov";
            paramProv.Value = provider;
            cmd.Parameters.Add(paramProv);

            var paramPuid = cmd.CreateParameter();
            paramPuid.ParameterName = "@puid";
            paramPuid.Value = providerUserId;
            cmd.Parameters.Add(paramPuid);

            var paramPid = cmd.CreateParameter();
            paramPid.ParameterName = "@pid";
            paramPid.Value = pid;
            cmd.Parameters.Add(paramPid);

            var paramTs = cmd.CreateParameter();
            paramTs.ParameterName = "@ts";
            paramTs.Value = createdAtUtc;
            cmd.Parameters.Add(paramTs);

            await cmd.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

    /// <summary>
    /// Asynchronously performs a bulk insert of playlist selection entries if they do not already exist for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="provider">The provider name.</param>
    /// <param name="providerUserId">The provider user identifier.</param>
    /// <param name="playlistIds">The collection of playlist identifiers to insert.</param>
    /// <param name="createdAtUtc">The creation timestamp in UTC.</param>
    /// <param name="conn">The database connection.</param>
    /// <param name="tx">The database transaction.</param>
    /// <returns>The number of rows inserted.</returns>
    public async Task<int> BulkInsertIfNotExistsAsync(
        string sessionId,
        string provider,
        string providerUserId,
        IEnumerable<string> playlistIds,
        DateTime createdAtUtc,
        DbConnection conn,
        DbTransaction tx)
    {
        const string sql = @"
        insert ignore into playlistselection (SessionId, Provider, ProviderUserId, PlaylistId, CreatedAt)
        values (@sid, @prov, @puid, @pid, @ts);";

        int inserted = 0;
        foreach (string pid in playlistIds.Distinct())
        {
            await using DbCommand cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;

            var paramSid = cmd.CreateParameter();
            paramSid.ParameterName = "@sid";
            paramSid.Value = sessionId;
            cmd.Parameters.Add(paramSid);

            var paramProv = cmd.CreateParameter();
            paramProv.ParameterName = "@prov";
            paramProv.Value = provider;
            cmd.Parameters.Add(paramProv);

            var paramPuid = cmd.CreateParameter();
            paramPuid.ParameterName = "@puid";
            paramPuid.Value = providerUserId;
            cmd.Parameters.Add(paramPuid);

            var paramPid = cmd.CreateParameter();
            paramPid.ParameterName = "@pid";
            paramPid.Value = pid;
            cmd.Parameters.Add(paramPid);

            var paramTs = cmd.CreateParameter();
            paramTs.ParameterName = "@ts";
            paramTs.Value = createdAtUtc;
            cmd.Parameters.Add(paramTs);

            int rows = await cmd.ExecuteNonQueryAsync(CancellationToken.None);
            if (rows > 0) inserted += rows;
        }

        return inserted;
    }

    /// <summary>
    /// Asynchronously performs a bulk delete of playlist selection entries by their identifiers for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="playlistIds">The collection of playlist identifiers to delete.</param>
    /// <param name="conn">The database connection.</param>
    /// <param name="tx">The database transaction.</param>
    /// <returns>The number of rows deleted.</returns>
    public async Task<int> BulkDeleteByIdsAsync(
        string sessionId,
        IEnumerable<string> playlistIds,
        DbConnection conn,
        DbTransaction tx)
    {
        const string sql = @"
        delete from playlistselection
        where SessionId = @sid and PlaylistId = @pid;";

        int deleted = 0;
        foreach (string pid in playlistIds.Distinct())
        {
            await using DbCommand cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;

            var paramSid = cmd.CreateParameter();
            paramSid.ParameterName = "@sid";
            paramSid.Value = sessionId;
            cmd.Parameters.Add(paramSid);

            var paramPid = cmd.CreateParameter();
            paramPid.ParameterName = "@pid";
            paramPid.Value = pid;
            cmd.Parameters.Add(paramPid);

            int rows = await cmd.ExecuteNonQueryAsync(CancellationToken.None);
            if (rows > 0) deleted += rows;
        }

        return deleted;
    }

    /// <summary>
    /// Asynchronously retrieves a read-only list of playlist identifiers for the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>A read-only list of playlist identifiers.</returns>
    public async Task<IReadOnlyList<string>> GetIdsBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        var ct = CancellationToken.None;
        await using DbConnection conn = await factory.CreateOpenAsync(ct);

        const string sql = @"
        select PlaylistId
        from playlistselection
        where SessionId = @sid
        order by PlaylistId;";

        await using DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var param = cmd.CreateParameter();
        param.ParameterName = "@sid";
        param.Value = sessionId;
        cmd.Parameters.Add(param);

        List<string> result = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }
}