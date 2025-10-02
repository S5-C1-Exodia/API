using MySqlConnector;

namespace API.Managers.InterfacesServices;

    /// <summary>
    /// Provides a method to create MySQL database connections.
    /// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Creates a new <see cref="MySqlConnection"/>.
    /// </summary>
    /// <returns>A new <see cref="MySqlConnection"/> instance.</returns>
    MySqlConnection Create();
}