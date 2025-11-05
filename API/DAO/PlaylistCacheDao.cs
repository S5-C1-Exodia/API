using System.Data.Common;
using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

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

        var ct = CancellationToken.None;
        await using DbConnection conn = await factory.CreateOpenAsync(ct);
        await using DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var param = cmd.CreateParameter();
        param.ParameterName = "@puid";
        param.Value = providerUserId;
        cmd.Parameters.Add(param);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteByProviderUserAsync(string providerUserId, DbConnection conn, DbTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from playlistcache where ProviderUserId = @puid";

        await using DbCommand cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;

        var param = cmd.CreateParameter();
        param.ParameterName = "@puid";
        param.Value = providerUserId;
        cmd.Parameters.Add(param);

        await cmd.ExecuteNonQueryAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task DeleteLinksBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = "delete from playlistcache_session where SessionId = @sid";

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
    public async Task DeleteLinksBySessionAsync(string sessionId, DbConnection conn, DbTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from playlistcache_session where SessionId = @sid";

        await using DbCommand cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;

        var param = cmd.CreateParameter();
        param.ParameterName = "@sid";
        param.Value = sessionId;
        cmd.Parameters.Add(param);

        await cmd.ExecuteNonQueryAsync(CancellationToken.None);
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

        await using DbConnection conn = await factory.CreateOpenAsync(ct);
        await using DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var pSid = cmd.CreateParameter();
        pSid.ParameterName = "@sid";
        pSid.Value = sessionId;
        cmd.Parameters.Add(pSid);

        var pPtok = cmd.CreateParameter();
        pPtok.ParameterName = "@ptok";
        pPtok.Value = (object?)pageToken ?? DBNull.Value;
        cmd.Parameters.Add(pPtok);

        var pNow = cmd.CreateParameter();
        pNow.ParameterName = "@now";
        pNow.Value = nowUtc;
        cmd.Parameters.Add(pNow);

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

        await using DbConnection conn = await factory.CreateOpenAsync(ct);
        await using DbTransaction tx = await conn.BeginTransactionAsync(ct);

        // Upsert cache row
        await using (DbCommand cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = upsertCache;

            var pPuid = cmd.CreateParameter();
            pPuid.ParameterName = "@puid";
            pPuid.Value = providerUserId;
            cmd.Parameters.Add(pPuid);

            var pPtok = cmd.CreateParameter();
            pPtok.ParameterName = "@ptok";
            pPtok.Value = (object?)pageToken ?? DBNull.Value;
            cmd.Parameters.Add(pPtok);

            var pJson = cmd.CreateParameter();
            pJson.ParameterName = "@json";
            pJson.Value = pageJson;
            cmd.Parameters.Add(pJson);

            var pNow = cmd.CreateParameter();
            pNow.ParameterName = "@now";
            pNow.Value = nowUtc;
            cmd.Parameters.Add(pNow);

            var pExp = cmd.CreateParameter();
            pExp.ParameterName = "@exp";
            pExp.Value = expiresAtUtc;
            cmd.Parameters.Add(pExp);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        // Upsert session link
        await using (DbCommand cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = upsertLink;

            var pSid = cmd.CreateParameter();
            pSid.ParameterName = "@sid";
            pSid.Value = sessionId;
            cmd.Parameters.Add(pSid);

            var pPuid = cmd.CreateParameter();
            pPuid.ParameterName = "@puid";
            pPuid.Value = providerUserId;
            cmd.Parameters.Add(pPuid);

            var pPtok = cmd.CreateParameter();
            pPtok.ParameterName = "@ptok";
            pPtok.Value = (object?)pageToken ?? DBNull.Value;
            cmd.Parameters.Add(pPtok);

            var pNow = cmd.CreateParameter();
            pNow.ParameterName = "@now";
            pNow.Value = nowUtc;
            cmd.Parameters.Add(pNow);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
    }
}