using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.DAO;

using System;
using System.Data;
using System.Threading.Tasks;

public class TokenDao(ISqlConnectionFactory factory, IClockService clock) : ITokenDao
{
    private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    private readonly IClockService _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    public async Task<long> SaveByStateAsync(string state, string provider, string providerUserId,
        string refreshTokenEnc, string scope, DateTime accessExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("provider cannot be null or empty.", nameof(provider));
        if (string.IsNullOrWhiteSpace(refreshTokenEnc))
            throw new ArgumentException("refreshTokenEnc cannot be null or empty.", nameof(refreshTokenEnc));

        DateTime now = this._clock.GetUtcNow();

        const string sql = @"
INSERT INTO TOKENSET (Provider, ProviderUserId, RefreshTokenEnc, Scope, AccessExpiresAt, UpdatedAt)
VALUES (@provider, @puid, @refresh, @scope, @accessExp, @updatedAt);
SELECT LAST_INSERT_ID();";

        MySqlConnection conn = this._factory.Create();
        try
        {
            await conn.OpenAsync();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@provider", provider);
            cmd.Parameters.AddWithValue("@puid", providerUserId ?? string.Empty);
            cmd.Parameters.AddWithValue("@refresh", refreshTokenEnc);
            cmd.Parameters.AddWithValue("@scope", scope ?? string.Empty);
            cmd.Parameters.AddWithValue("@accessExp", accessExpiresAt);
            cmd.Parameters.AddWithValue("@updatedAt", now);

            object scalar = await cmd.ExecuteScalarAsync();
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

    public async Task AttachToSessionAsync(long tokenSetId, string sessionId)
    {
        if (tokenSetId <= 0)
            throw new ArgumentException("tokenSetId must be positive.", nameof(tokenSetId));

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        DateTime now = this._clock.GetUtcNow();

        const string sql = @"
UPDATE TOKENSET
SET SessionId = @sid, UpdatedAt = @updatedAt
WHERE TokenSetId = @id";

        MySqlConnection conn = this._factory.Create();
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
}