using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using MySqlConnector;

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

        await using var conn = factory.Create();
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

        const string sql = "delete from accesstoken where SessionId = @sid";

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
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

        MySqlConnection conn = factory.Create();
        try
        {
            await conn.OpenAsync(ct);
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@sid", sessionId);
            cmd.Parameters.AddWithValue("@now", nowUtc);

            object? scalar = await cmd.ExecuteScalarAsync(ct);
            if (scalar is null || scalar == DBNull.Value) return null;
            return Convert.ToString(scalar);
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
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

        MySqlConnection conn = factory.Create();
        try
        {
            await conn.OpenAsync(ct);
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@sid", sessionId);
            cmd.Parameters.AddWithValue("@tok", accessToken);
            cmd.Parameters.AddWithValue("@exp", expiresAtUtc);
            cmd.Parameters.AddWithValue("@now", nowUtc);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }
}