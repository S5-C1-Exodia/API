using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.DAO;

/// <summary>
/// MySQL implementation for the DENYLISTEDREFRESH table.
/// Provides methods to check existence and upsert denylisted refresh tokens.
/// </summary>
public class DenylistedRefreshDao : IDenylistedRefreshDao
{
    private readonly ISqlConnectionFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DenylistedRefreshDao"/> class.
    /// </summary>
    /// <param name="factory">The SQL connection factory used to create database connections.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is null.</exception>
    public DenylistedRefreshDao(ISqlConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string refreshHash, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(refreshHash))
            throw new ArgumentException("refreshHash cannot be null or empty.", nameof(refreshHash));

        const string sql = @"
        SELECT 1
        FROM DENYLISTEDREFRESH
        WHERE RefreshHash = @h AND ExpiresAt > @now
        LIMIT 1;";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@h", refreshHash);
        cmd.Parameters.AddWithValue("@now", nowUtc);

        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }

    /// <inheritdoc />
    public async Task UpsertAsync(string refreshHash, string reason, DateTime addedAtUtc, DateTime expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(refreshHash))
            throw new ArgumentException("refreshHash cannot be null or empty.", nameof(refreshHash));

        const string sql = @"
        INSERT INTO DENYLISTEDREFRESH(RefreshHash, Reason, AddedAt, ExpiresAt)
        VALUES (@h, @r, @added, @exp)
        ON DUPLICATE KEY UPDATE
            Reason = VALUES(Reason),
            AddedAt = VALUES(AddedAt),
            ExpiresAt = VALUES(ExpiresAt);";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@h", refreshHash);
        cmd.Parameters.AddWithValue("@r", reason ?? "logout");
        cmd.Parameters.AddWithValue("@added", addedAtUtc);
        cmd.Parameters.AddWithValue("@exp", expiresAtUtc);
        await cmd.ExecuteNonQueryAsync();
    }
}