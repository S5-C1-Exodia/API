using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using Api.Models;
using MySqlConnector;

namespace API.DAO;

using System;
using System.Data;
using System.Threading.Tasks;

/// <summary>
/// Data Access Object for managing TokenSet entries in the database.
/// Provides methods to save, attach, retrieve, and delete token sets by state or session.
/// </summary>
public class TokenDao(ISqlConnectionFactory factory, IClockService clock) : ITokenDao
{
    private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    private readonly IClockService _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public async Task<long> SaveByStateAsync(string state, string provider, string providerUserId,
        string refreshTokenEnc, string scope, DateTime accessExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("provider cannot be null or empty.", nameof(provider));
        if (string.IsNullOrWhiteSpace(refreshTokenEnc))
            throw new ArgumentException("refreshTokenEnc cannot be null or empty.", nameof(refreshTokenEnc));

        DateTime now = _clock.GetUtcNow();

        const string sql = @"
INSERT INTO tokenset (Provider, ProviderUserId, RefreshTokenEnc, Scope, AccessExpiresAt, UpdatedAt)
VALUES (@provider, @puid, @refresh, @scope, @accessExp, @updatedAt);
SELECT LAST_INSERT_ID();";

        MySqlConnection conn = _factory.Create();
        try
        {
            await conn.OpenAsync();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@provider", provider);
            cmd.Parameters.AddWithValue("@puid", providerUserId);
            cmd.Parameters.AddWithValue("@refresh", refreshTokenEnc);
            cmd.Parameters.AddWithValue("@scope", scope);
            cmd.Parameters.AddWithValue("@accessExp", accessExpiresAt);
            cmd.Parameters.AddWithValue("@updatedAt", now);

            object scalar = await cmd.ExecuteScalarAsync() ?? throw new DataException("Failed to execute insert for TOKENSET.");
            if (scalar == null || scalar == DBNull.Value)
                throw new DataException("Failed to retrieve LAST_INSERT_ID for TOKENSET.");

            bool ok = long.TryParse(Convert.ToString(scalar), out long id);
            if (!ok || id <= 0) throw new DataException("Invalid LAST_INSERT_ID for TOKENSET.");
            return id;
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async Task AttachToSessionAsync(long tokenSetId, string sessionId)
    {
        if (tokenSetId <= 0)
            throw new ArgumentException("tokenSetId must be positive.", nameof(tokenSetId));

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        DateTime now = _clock.GetUtcNow();

        const string sql = @"
UPDATE tokenset
SET SessionId = @sid, UpdatedAt = @updatedAt
WHERE TokenSetId = @id";

        MySqlConnection conn = _factory.Create();
        try
        {
            await conn.OpenAsync();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@sid", sessionId);
            cmd.Parameters.AddWithValue("@updatedAt", now);
            cmd.Parameters.AddWithValue("@id", tokenSetId);

            int affected = await cmd.ExecuteNonQueryAsync();
            if (affected != 1)
                throw new DataException("Unexpected number of rows updated for TOKENSET.AttachToSession.");
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async Task<TokenSet?> GetBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = @"
SELECT TokenSetId, Provider, ProviderUserId, RefreshTokenEnc, Scope, AccessExpiresAt, UpdatedAt, SessionId
FROM tokenset
WHERE SessionId = @sid
LIMIT 1";

        await using MySqlConnection conn = _factory.Create();
        await conn.OpenAsync();
        await using MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);

        await using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;

        if (await reader.ReadAsync())
        {
            return new TokenSet(
                tokenSetId: reader.GetInt64("TokenSetId"),
                provider: reader.GetString("Provider"),
                providerUserId: reader.GetString("ProviderUserId"),
                refreshToken: reader.GetString("RefreshTokenEnc"),
                scope: reader.IsDBNull(reader.GetOrdinal("Scope")) ? string.Empty : reader.GetString("Scope"),
                accessExpiresAt: reader.GetDateTime("AccessExpiresAt"),
                updatedAt: reader.GetDateTime("UpdatedAt"),
                sessionId: (reader.IsDBNull(reader.GetOrdinal("SessionId")) ? null : reader.GetString("SessionId")) ??
                           throw new InvalidOperationException("SessionId should not be null here.")
            );
        }

        return null;
    }

    /// <inheritdoc />
    public async Task DeleteBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = "delete from tokenset where SessionId = @sid";

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
        if (conn is null || tx is null)
            throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from tokenset where SessionId = @sid";

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAfterRefreshAsync(string sessionId, string refreshToken, DateTime newAccessExpiresAtUtc,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("refreshToken cannot be null or empty.", nameof(refreshToken));

        DateTime now = _clock.GetUtcNow();

        const string sql = @"
update tokenset
set RefreshTokenEnc = @refresh,
    AccessExpiresAt = @accessExp,
    UpdatedAt       = @updatedAt
where SessionId = @sid
limit 1;";

        MySqlConnection conn = _factory.Create();
        try
        {
            await conn.OpenAsync(ct);
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@refresh", refreshToken);
            cmd.Parameters.AddWithValue("@accessExp", newAccessExpiresAtUtc);
            cmd.Parameters.AddWithValue("@updatedAt", now);
            cmd.Parameters.AddWithValue("@sid", sessionId);

            await cmd.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }
}