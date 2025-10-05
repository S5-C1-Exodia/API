using MySqlConnector;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for playlist cache data access operations.
/// </summary>
public interface IPlaylistCacheDao
{
    /// <summary>
    /// Deletes all playlist cache entries for a given provider user asynchronously.
    /// </summary>
    /// <param name="providerUserId">The provider user identifier.</param>
    Task DeleteByProviderUserAsync(string providerUserId);

    /// <summary>
    /// Deletes all playlist cache entries for a given provider user within a transaction asynchronously.
    /// </summary>
    /// <param name="providerUserId">The provider user identifier.</param>
    /// <param name="conn">The MySQL connection.</param>
    /// <param name="tx">The MySQL transaction.</param>
    Task DeleteByProviderUserAsync(string providerUserId, MySqlConnection conn, MySqlTransaction tx);

    /// <summary>
    /// Deletes all playlist links for a given session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    Task DeleteLinksBySessionAsync(string sessionId);

    /// <summary>
    /// Deletes all playlist links for a given session within a transaction asynchronously.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="conn">The MySQL connection.</param>
    /// <param name="tx">The MySQL transaction.</param>
    Task DeleteLinksBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);
}