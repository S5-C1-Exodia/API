using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.DAO;

public class AccessTokenDao(ISqlConnectionFactory factory) : IAccessTokenDao
{
    private readonly ISqlConnectionFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public async Task DeleteBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        const string sql = "DELETE FROM ACCESSTOKEN WHERE SessionId = @sid";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }
}