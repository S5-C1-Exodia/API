using API.Managers.InterfacesServices;

namespace API.Services
{
    public class TokenDenyListService(ISqlConnectionFactory factory) : ITokenDenyListService
    {
        private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        public async Task AddAsync(string refreshTokenHash, string reason, DateTime? expiresAtUtc)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenHash))
                throw new ArgumentException("refreshTokenHash cannot be null or empty.", nameof(refreshTokenHash));

            var exp = expiresAtUtc ?? DateTime.UtcNow.AddDays(90);
            const string sql = @"
INSERT INTO DENYLISTEDREFRESH (RefreshHash, Reason, AddedAt, ExpiresAt)
VALUES (@hash, @reason, @now, @exp)
ON DUPLICATE KEY UPDATE Reason = VALUES(Reason), ExpiresAt = VALUES(ExpiresAt)";

            await using var conn = _factory.Create();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@hash", refreshTokenHash);
            cmd.Parameters.AddWithValue("@reason", reason ?? "logout");
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@exp", exp);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsDeniedAsync(string refreshTokenHash)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenHash))
                return false;

            const string sql = @"
SELECT 1
FROM DENYLISTEDREFRESH
WHERE RefreshHash = @hash AND ExpiresAt > @now
LIMIT 1";

            await using var conn = _factory.Create();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@hash", refreshTokenHash);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

            var scalar = await cmd.ExecuteScalarAsync();
            return scalar != null;
        }
    }
}