using MySqlConnector;

namespace Api.Managers.InterfacesDao;

public interface IPlaylistCacheDao
{
    Task DeleteByProviderUserAsync(string providerUserId);
    Task DeleteByProviderUserAsync(string providerUserId, MySqlConnection conn, MySqlTransaction tx);

    Task DeleteLinksBySessionAsync(string sessionId);
    Task DeleteLinksBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);
}