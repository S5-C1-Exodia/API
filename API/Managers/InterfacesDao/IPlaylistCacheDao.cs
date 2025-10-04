namespace Api.Managers.InterfacesDao;

public interface IPlaylistCacheDao
{
    // Purge tout le cache des playlists d’un user (ProviderUserId)
    Task DeleteByProviderUserAsync(string providerUserId);

    // Supprime uniquement les liens session<->cache (table de liaison)
    Task DeleteLinksBySessionAsync(string sessionId);
}