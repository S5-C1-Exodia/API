using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.Services;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString cannot be null or empty.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public MySqlConnection Create()
    {
        return new MySqlConnection(_connectionString);
    }
}