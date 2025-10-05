using MySqlConnector;

namespace Api.Managers.InterfacesDao;

public interface IAccessTokenDao
{
    Task DeleteBySessionAsync(string sessionId);
    Task DeleteBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);
}