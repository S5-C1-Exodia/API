using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.Services;

public class MySqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString cannot be null or empty.", nameof(connectionString));
        this._connectionString = connectionString;
    }

    public MySqlConnection Create()
    {
        return new MySqlConnection(this._connectionString);
    }
}