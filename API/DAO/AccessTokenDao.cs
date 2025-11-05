using System.Data.Common;
using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.DAO;

/// <summary>
/// Data Access Object for managing access tokens in the database.
/// Provides methods to delete access tokens by session identifier.
/// </summary>
public class AccessTokenDao(ISqlConnectionFactory factory) : IAccessTokenDao
{
    /// <inheritdoc />
    public async Task DeleteBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = "delete from accesstoken where SessionId = @sid";

        var ct = CancellationToken.None;
        await using DbConnection conn = await factory.CreateOpenAsync(ct);
        await using var cmd = conn.CreateCommand();
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

        const string sql = "delete from accesstoken where SessionId = @sid";

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;

        var param = cmd.CreateParameter();
        param.ParameterName = "@sid";
        param.Value = sessionId;
        cmd.Parameters.Add(param);

        await cmd.ExecuteNonQueryAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<string?> GetValidBySessionAsync(string sessionId, DateTime nowUtc, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = @"
        select AccessTokenEnc
        from accesstoken
        where SessionId = @sid
          and ExpiresAt > @now
        order by ExpiresAt desc
        limit 1;";

        await using DbConnection conn = await factory.CreateOpenAsync(ct);
        string? result = null;

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var pSid = cmd.CreateParameter();
        pSid.ParameterName = "@sid";
        pSid.Value = sessionId;
        cmd.Parameters.Add(pSid);

        var pNow = cmd.CreateParameter();
        pNow.ParameterName = "@now";
        pNow.Value = nowUtc;
        cmd.Parameters.Add(pNow);

        object? scalar = await cmd.ExecuteScalarAsync(ct);
        if (!(scalar is null || scalar == DBNull.Value))
            result = Convert.ToString(scalar);

        return result;
    }

    /// <inheritdoc />
    public async Task UpsertAsync(string sessionId, string accessToken, DateTime expiresAtUtc, DateTime nowUtc,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("accessToken cannot be null or empty.", nameof(accessToken));

        const string sql = @"
        insert into accesstoken (SessionId, AccessTokenEnc, ExpiresAt, CreatedAt)
        values (@sid, @tok, @exp, @now)
        on duplicate key update
          AccessTokenEnc = values(AccessTokenEnc),
          ExpiresAt     = values(ExpiresAt),
          CreatedAt     = values(CreatedAt);";

        await using DbConnection conn = await factory.CreateOpenAsync(ct);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var pSid = cmd.CreateParameter();
        pSid.ParameterName = "@sid";
        pSid.Value = sessionId;
        cmd.Parameters.Add(pSid);

        var pTok = cmd.CreateParameter();
        pTok.ParameterName = "@tok";
        pTok.Value = accessToken;
        cmd.Parameters.Add(pTok);

        var pExp = cmd.CreateParameter();
        pExp.ParameterName = "@exp";
        pExp.Value = expiresAtUtc;
        cmd.Parameters.Add(pExp);

        var pNow = cmd.CreateParameter();
        pNow.ParameterName = "@now";
        pNow.Value = nowUtc;
        cmd.Parameters.Add(pNow);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}