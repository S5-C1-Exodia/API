using MySqlConnector;

namespace Api.Managers.InterfacesDao;

public interface IPlaylistSelectionDao
{
    Task DeleteBySessionAsync(string sessionId);
    Task DeleteBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);
}