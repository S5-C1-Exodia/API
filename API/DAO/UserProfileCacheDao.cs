using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.DAO;

/// <summary>
/// Data Access Object for managing user profile cache entries in the database.
/// Provides methods to delete user profile cache entries by provider user identifier.
/// </summary>
public class UserProfileCacheDao(ISqlConnectionFactory factory) : IUserProfileCacheDao
{
    /// <inheritdoc />
    public async Task DeleteByProviderUserAsync(string providerUserId)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));

        const string sql = "delete from userprofilecache where ProviderUserId = @puid";

        await using var conn = factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@puid", providerUserId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <inheritdoc />
    public async Task DeleteByProviderUserAsync(string providerUserId, MySqlConnection conn, MySqlTransaction tx)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));
        if (conn is null || tx is null) throw new ArgumentNullException(nameof(conn));

        const string sql = "delete from userprofilecache where ProviderUserId = @puid";

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@puid", providerUserId);
        await cmd.ExecuteNonQueryAsync();
    }
}