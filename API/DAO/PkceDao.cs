using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using Api.Models;
using MySqlConnector;

namespace API.DAO;

using System;
using System.Data;
using System.Threading.Tasks;

public class PkceDao : IPkceDao
{
    private readonly ISqlConnectionFactory _factory;

    public PkceDao(ISqlConnectionFactory factory)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        this._factory = factory;
    }

    public async Task SaveAsync(PkceEntry entry)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        const string sql = @"
INSERT INTO PKCEENTRY (State, CodeVerifier, CodeChallenge, ExpiresAt)
VALUES (@state, @verifier, @challenge, @exp)";

        MySqlConnection conn = this._factory.Create();
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
            {
                throw new DataException("Unexpected number of rows inserted for PKCEENTRY.");
            }
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }

    public async Task<PkceEntry> GetAsync(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("state cannot be null or empty.", nameof(state));
        }

        const string sql = @"
SELECT State, CodeVerifier, CodeChallenge, ExpiresAt
FROM PKCEENTRY
WHERE State = @state
LIMIT 1";

        MySqlConnection conn = this._factory.Create();
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
                {
                    return null;
                }

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

    public async Task DeleteAsync(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("state cannot be null or empty.", nameof(state));
        }

        const string sql = @"DELETE FROM PKCEENTRY WHERE State = @state";

        MySqlConnection conn = this._factory.Create();
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