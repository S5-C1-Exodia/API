using MySqlConnector;

namespace API.Managers.InterfacesServices;

public interface ISqlConnectionFactory
{
    MySqlConnection Create();
}