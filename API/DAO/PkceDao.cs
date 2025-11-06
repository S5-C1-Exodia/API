using System.Data;
using System.Data.Common;
using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using Api.Models;

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

    /// <inheritdoc />
    public async Task SaveAsync(PkceEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        const string sql = @"
        INSERT INTO pkceentry (State, CodeVerifier, CodeChallenge, ExpiresAt)
        VALUES (@state, @verifier, @challenge, @exp)";

        var ct = CancellationToken.None;
        await using DbConnection conn = await _factory.CreateOpenAsync(ct);
        await using DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var paramState = cmd.CreateParameter();
        paramState.ParameterName = "@state";
        paramState.Value = entry.State;
        cmd.Parameters.Add(paramState);

        var paramVerifier = cmd.CreateParameter();
        paramVerifier.ParameterName = "@verifier";
        paramVerifier.Value = entry.CodeVerifier;
        cmd.Parameters.Add(paramVerifier);

        var paramChallenge = cmd.CreateParameter();
        paramChallenge.ParameterName = "@challenge";
        paramChallenge.Value = entry.CodeChallenge;
        cmd.Parameters.Add(paramChallenge);

        var paramExp = cmd.CreateParameter();
        paramExp.ParameterName = "@exp";
        paramExp.Value = entry.ExpiresAt;
        cmd.Parameters.Add(paramExp);

        int affected = await cmd.ExecuteNonQueryAsync(ct);
        if (affected != 1)
            throw new DataException("Unexpected number of rows inserted for PKCEENTRY.");
    }

    /// <inheritdoc />
    public async Task<PkceEntry?> GetAsync(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("state cannot be null or empty.", nameof(state));

        const string sql = @"
    SELECT State, CodeVerifier, CodeChallenge, ExpiresAt
    FROM pkceentry
    WHERE State = @state
    LIMIT 1";

        var ct = CancellationToken.None;
        await using DbConnection conn = await _factory.CreateOpenAsync(ct);
        PkceEntry? result = null;

        await using DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var paramState = cmd.CreateParameter();
        paramState.ParameterName = "@state";
        paramState.Value = state;
        cmd.Parameters.Add(paramState);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync())
        {
            string s = reader.GetString("State");
            string verifier = reader.GetString("CodeVerifier");
            string challenge = reader.GetString("CodeChallenge");
            DateTime exp = reader.GetDateTime("ExpiresAt");
            result = new PkceEntry(s, verifier, challenge, exp);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("state cannot be null or empty.", nameof(state));
        }

        const string sql = @"DELETE FROM pkceentry WHERE State = @state";

        var ct = CancellationToken.None;
        await using DbConnection conn = await _factory.CreateOpenAsync(ct);
        await using DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var paramState = cmd.CreateParameter();
        paramState.ParameterName = "@state";
        paramState.Value = state;
        cmd.Parameters.Add(paramState);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}