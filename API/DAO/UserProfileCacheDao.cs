using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.DAO;

public class UserProfileCacheDao(ISqlConnectionFactory factory) : IUserProfileCacheDao
{
    private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public async Task DeleteByProviderUserAsync(string providerUserId)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));

        const string sql = "DELETE FROM USERPROFILECACHE WHERE ProviderUserId = @puid";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@puid", providerUserId);
        await cmd.ExecuteNonQueryAsync();
    }
}