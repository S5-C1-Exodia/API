using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.DAO;

public class PlaylistCacheDao : IPlaylistCacheDao
{
    private readonly ISqlConnectionFactory _factory;

    public PlaylistCacheDao(ISqlConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task DeleteByProviderUserAsync(string providerUserId)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new ArgumentException("providerUserId cannot be null or empty.", nameof(providerUserId));

        // PLAYLISTCACHE (table principale : ProviderUserId + PlaylistId)
        const string sql = "DELETE FROM PLAYLISTCACHE WHERE ProviderUserId = @puid";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@puid", providerUserId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteLinksBySessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        // Table de liaison pour purge ciblée par session
        const string sql = "DELETE FROM PLAYLISTCACHE_SESSION WHERE SessionId = @sid";

        await using var conn = _factory.Create();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@sid", sessionId);
        await cmd.ExecuteNonQueryAsync();
    }
}