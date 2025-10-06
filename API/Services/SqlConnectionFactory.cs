using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.Services;

/// <inheritdoc />
public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString cannot be null or empty.", nameof(connectionString));
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public MySqlConnection Create() => new MySqlConnection(_connectionString);
}