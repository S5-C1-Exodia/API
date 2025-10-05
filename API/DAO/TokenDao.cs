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

    /// <summary>
    /// Saves a new TokenSet entry in the database by state and returns its generated ID.
    /// </summary>
    /// <param name="state">The state associated with the token set.</param>
    /// <param name="provider">The provider name (must not be null or empty).</param>
    /// <param name="providerUserId">The provider user identifier.</param>
    /// <param name="refreshTokenEnc">The encrypted refresh token (must not be null or empty).</param>
    /// <param name="scope">The scope of the token.</param>
    /// <param name="accessExpiresAt">The expiration date and time of the access token.</param>
    /// <returns>The ID of the newly inserted TokenSet.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="provider"/> or <paramref name="refreshTokenEnc"/> is null or empty.</exception>
    /// <exception cref="DataException">Thrown if the insert fails or the returned ID is invalid.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task<long> SaveByStateAsync(string state, string provider, string providerUserId,
        string refreshTokenEnc, string scope, DateTime accessExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("provider cannot be null or empty.", nameof(provider));
        if (string.IsNullOrWhiteSpace(refreshTokenEnc))
            throw new ArgumentException("refreshTokenEnc cannot be null or empty.", nameof(refreshTokenEnc));

        DateTime now = _clock.GetUtcNow();

        const string sql = @"
INSERT INTO TOKENSET (Provider, ProviderUserId, RefreshTokenEnc, Scope, AccessExpiresAt, UpdatedAt)
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

    /// <summary>
    /// Attaches a TokenSet to a session by updating its SessionId and UpdatedAt fields.
    /// </summary>
    /// <param name="tokenSetId">The ID of the TokenSet to attach (must be positive).</param>
    /// <param name="sessionId">The session identifier (must not be null or empty).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="tokenSetId"/> is not positive or <paramref name="sessionId"/> is null or empty.</exception>
    /// <exception cref="DataException">Thrown if the update does not affect exactly one row.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task AttachToSessionAsync(long tokenSetId, string sessionId)
    {
        if (tokenSetId <= 0)
            throw new ArgumentException("tokenSetId must be positive.", nameof(tokenSetId));

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        DateTime now = _clock.GetUtcNow();

        const string sql = @"
UPDATE TOKENSET
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

    /// <summary>
    /// Retrieves a TokenSet entry from the database by its session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier to search for (must not be null or empty).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the <see cref="TokenSet"/>
    /// if found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if SessionId is unexpectedly null in the database.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task<TokenSet?> GetBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = @"
SELECT TokenSetId, Provider, ProviderUserId, RefreshTokenEnc, Scope, AccessExpiresAt, UpdatedAt, SessionId
FROM TOKENSET
WHERE SessionId = @sid
LIMIT 1";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;

        if (await reader.ReadAsync())
        {
            return new TokenSet(
                tokenSetId: reader.GetInt64("TokenSetId"),
                provider: reader.GetString("Provider"),
                providerUserId: reader.GetString("ProviderUserId"),
                refreshTokenEnc: reader.GetString("RefreshTokenEnc"),
                scope: reader.IsDBNull(reader.GetOrdinal("Scope")) ? string.Empty : reader.GetString("Scope"),
                accessExpiresAt: reader.GetDateTime("AccessExpiresAt"),
                updatedAt: reader.GetDateTime("UpdatedAt"),
                sessionId: (reader.IsDBNull(reader.GetOrdinal("SessionId")) ? null : reader.GetString("SessionId")) ??
                           throw new InvalidOperationException("SessionId should not be null here.")
            );
        }

        return null;
    }

    /// <summary>
    /// Deletes all TokenSet entries associated with the specified session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier whose token sets should be deleted (must not be null or empty).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
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

    /// <summary>
    /// Deletes all TokenSet entries associated with the specified session identifier,
    /// using an existing MySQL connection and transaction.
    /// </summary>
    /// <param name="sessionId">The session identifier whose token sets should be deleted (must not be null or empty).</param>
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
        if (conn is null || tx is null)
            throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from tokenset where SessionId = @sid";

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }
}