namespace Api.Managers.InterfacesDao
{
    /// <summary>
    /// DAO d'accès à la table DENYLISTEDREFRESH (accès pur aux données).
    /// </summary>
    public interface IDenylistedRefreshDao
    {
        /// <summary>
        /// Indique si un hash de refresh est encore actif (non expiré) dans la denylist.
        /// </summary>
        Task<bool> ExistsAsync(string refreshHash, DateTime nowUtc);

        /// <summary>
        /// Insère ou met à jour une entrée de denylist.
        /// </summary>
        Task UpsertAsync(string refreshHash, string reason, DateTime addedAtUtc, DateTime expiresAtUtc);
    }
}