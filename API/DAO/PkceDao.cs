using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using Api.Models;
using MySqlConnector;
using System.Data;

namespace API.DAO;

/// <summary>
/// Data Access Object for PKCE entries.
/// Provides methods to save, retrieve, and delete PKCE entries in the database.
/// </summary>
public class PkceDao : IPkceDao
{
    private readonly ISqlConnectionFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PkceDao"/> class.
    /// </summary>
    /// <param name="factory">The SQL connection factory used to create database connections.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is null.</exception>
    public PkceDao(ISqlConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Saves a PKCE entry to the database.
    /// </summary>
    /// <param name="entry">The <see cref="PkceEntry"/> to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.</exception>
    /// <exception cref="DataException">Thrown if the number of affected rows is not 1.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task SaveAsync(PkceEntry? entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        const string sql = @"
    INSERT INTO PKCEENTRY (State, CodeVerifier, CodeChallenge, ExpiresAt)
    VALUES (@state, @verifier, @challenge, @exp)";

        MySqlConnection conn = _factory.Create();
        try
        {
            await conn.OpenAsync();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@state", entry.State);
            cmd.Parameters.AddWithValue("@verifier", entry.CodeVerifier);
            cmd.Parameters.AddWithValue("@challenge", entry.CodeChallenge);
            cmd.Parameters.AddWithValue("@exp", entry.ExpiresAt);
            int affected = await cmd.ExecuteNonQueryAsync();
            if (affected != 1)
                throw new DataException("Unexpected number of rows inserted for PKCEENTRY."); 
        }
        
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }

    /// <summary>
    /// Retrieves a PKCE entry from the database by its state.
    /// </summary>
    /// <param name="state">The state identifier of the PKCE entry.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the <see cref="PkceEntry"/>
    /// if found; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="state"/> is null or empty.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task<PkceEntry?> GetAsync(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("state cannot be null or empty.", nameof(state));

        const string sql = @"
    SELECT State, CodeVerifier, CodeChallenge, ExpiresAt
    FROM PKCEENTRY
    WHERE State = @state
    LIMIT 1";

        MySqlConnection conn = _factory.Create();
        try
        {
            await conn.OpenAsync();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@state", state);

            MySqlDataReader reader = await cmd.ExecuteReaderAsync();
            try
            {
                if (!reader.HasRows)
                    return null;

                if (await reader.ReadAsync())
                {
                    string s = reader.GetString("State");
                    string verifier = reader.GetString("CodeVerifier");
                    string challenge = reader.GetString("CodeChallenge");
                    DateTime exp = reader.GetDateTime("ExpiresAt");
                    PkceEntry entry = new PkceEntry(s, verifier, challenge, exp);
                    return entry;
                }

                return null;
            }
            finally
            {
                await reader.DisposeAsync();
            }
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }

    /// <summary>
    /// Deletes a PKCE entry from the database by its state.
    /// </summary>
    /// <param name="state">The state identifier of the PKCE entry to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="state"/> is null or empty.</exception>
    /// <exception cref="MySqlException">Thrown if a database error occurs during execution.</exception>
    public async Task DeleteAsync(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("state cannot be null or empty.", nameof(state));
        }

        const string sql = @"DELETE FROM PKCEENTRY WHERE State = @state";

        MySqlConnection conn = _factory.Create();
        try
        {
            await conn.OpenAsync();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@state", state);
            await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }
}