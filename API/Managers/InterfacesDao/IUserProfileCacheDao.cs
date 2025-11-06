using System.Data.Common;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for user profile cache data access operations.
/// </summary>
public interface IUserProfileCacheDao
{

    /// <summary>
    /// Deletes the user profile cache for a given provider user within a transaction asynchronously.
    /// </summary>
    /// <param name="providerUserId">The provider user identifier.</param>
    /// <param name="conn">The database connection to use for the operation.</param>
    /// <param name="tx">The database transaction to use for the operation.</param>
    Task DeleteByProviderUserAsync(string providerUserId, DbConnection conn, DbTransaction tx);
}