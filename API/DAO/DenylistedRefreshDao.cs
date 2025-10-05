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

    /// <summary>
    /// Checks if a denylisted refresh token exists and is not expired.
    /// </summary>
    /// <param name="refreshHash">The hash of the refresh token to check.</param>
    /// <param name="nowUtc">The current UTC date and time for expiration comparison.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// <c>true</c> if the refresh token exists and is not expired; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="refreshHash"/> is null or empty.
    /// </exception>
    /// <exception cref="MySqlConnector.MySqlException">
    /// Thrown if a database error occurs during execution.
    /// </exception>
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

    /// <summary>
    /// Inserts or updates a denylisted refresh token entry in the database.
    /// </summary>
    /// <param name="refreshHash">The hash of the refresh token to upsert.</param>
    /// <param name="reason">The reason for denylisting the token. If null, defaults to "logout".</param>
    /// <param name="addedAtUtc">The UTC date and time when the token was added to the denylist.</param>
    /// <param name="expiresAtUtc">The UTC date and time when the denylist entry expires.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="refreshHash"/> is null or empty.
    /// </exception>
    /// <exception cref="MySqlConnector.MySqlException">
    /// Thrown if a database error occurs during execution.
    /// </exception>
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