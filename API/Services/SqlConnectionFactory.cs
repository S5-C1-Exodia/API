using API.Managers.InterfacesServices;
using MySqlConnector;

namespace API.Services;

/// <summary>
/// Factory for creating MySQL database connections.
/// </summary>
/// <param name="connectionString">The connection string used to establish database connections.</param>
/// <exception cref="ArgumentException">Thrown if the connection string is null or empty.</exception>
public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string used to establish database connections.</param>
    /// <exception cref="ArgumentException">Thrown if the connection string is null or empty.</exception>
    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString cannot be null or empty.", nameof(connectionString));
        _connectionString = connectionString;
    }

    /// <summary>
    /// Creates a new MySQL connection using the provided connection string.
    /// </summary>
    /// <returns>A new instance of <see cref="MySqlConnection"/>.</returns>
    public MySqlConnection Create()
    {
        return new MySqlConnection(_connectionString);
    }
}