using MySqlConnector;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for user profile cache data access operations.
/// </summary>
public interface IUserProfileCacheDao
{
    /// <summary>
    /// Deletes the user profile cache for a given provider user asynchronously.
    /// </summary>
    /// <param name="providerUserId">The provider user identifier.</param>
    Task DeleteByProviderUserAsync(string providerUserId);

    /// <summary>
    /// Deletes the user profile cache for a given provider user within a transaction asynchronously.
    /// </summary>
    /// <param name="providerUserId">The provider user identifier.</param>
    /// <param name="conn">The MySQL connection.</param>
    /// <param name="tx">The MySQL transaction.</param>
    Task DeleteByProviderUserAsync(string providerUserId, MySqlConnection conn, MySqlTransaction tx);
}