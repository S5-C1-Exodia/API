using System.Data;
using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using Api.Models;
using MySqlConnector;

namespace API.DAO;

public class SessionDao(ISqlConnectionFactory factory) : ISessionDao
{
    private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public async Task InsertAsync(AppSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        const string sql = @"
INSERT INTO APPSESSION (SessionId, DeviceInfo, CreatedAt, LastSeenAt, ExpiresAt)
VALUES (@sid, @device, @created, @lastseen, @expires)";

        MySqlConnection conn = this._factory.Create();
        try
        {
            await conn.OpenAsync();

            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@sid", session.SessionId);
            cmd.Parameters.AddWithValue("@device", session.DeviceInfo ?? string.Empty);
            cmd.Parameters.AddWithValue("@created", session.CreatedAt);
            cmd.Parameters.AddWithValue("@lastseen", session.LastSeenAt);
            cmd.Parameters.AddWithValue("@expires", session.ExpiresAt);

            int affected = await cmd.ExecuteNonQueryAsync();
            if (affected != 1)
                throw new DataException("Unexpected number of rows inserted for APPSESSION.");
        }
        finally
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }

    public async Task<AppSession> GetAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = @"
SELECT SessionId, DeviceInfo, CreatedAt, LastSeenAt, ExpiresAt
FROM APPSESSION
WHERE SessionId = @sid
LIMIT 1";

        MySqlConnection conn = this._factory.Create();
        try
        {
            await conn.OpenAsync();

            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@sid", sessionId);

            MySqlDataReader reader = await cmd.ExecuteReaderAsync();
            try
            {
                if (!reader.HasRows) return null;

                if (await reader.ReadAsync())
                {
                    string sid = reader.GetString("SessionId");
                    string device = reader.IsDBNull(reader.GetOrdinal("DeviceInfo"))
                        ? string.Empty
                        : reader.GetString("DeviceInfo");
                    DateTime created = reader.GetDateTime("CreatedAt");
                    DateTime lastSeen = reader.GetDateTime("LastSeenAt");
                    DateTime expires = reader.GetDateTime("ExpiresAt");

                    AppSession session = new AppSession(sid, device, created, lastSeen, expires);
                    return session;
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
}