using MySqlConnector;

namespace Api.Managers.InterfacesDao;

public interface IUserProfileCacheDao
{
    Task DeleteByProviderUserAsync(string providerUserId);
    Task DeleteByProviderUserAsync(string providerUserId, MySqlConnection conn, MySqlTransaction tx);
}