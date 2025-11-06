namespace Api.Managers.InterfacesDao
{
    /// <summary>
    /// Defines data access operations for the DENYLISTEDREFRESH table (pure data access).
    /// </summary>
    public interface IDenylistedRefreshDao
    {
        /// <summary>
        /// Asynchronously determines whether a refresh hash is still active (not expired) in the denylist.
        /// </summary>
        /// <param name="refreshHash">The hash of the refresh token to check. Must not be null or empty.</param>
        /// <param name="nowUtc">The current UTC date and time used to determine expiration.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> representing the asynchronous operation, containing <c>true</c> if the hash exists and is not expired; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="refreshHash"/> is null or empty.</exception>
        Task<bool> ExistsAsync(string refreshHash, DateTime nowUtc);

        /// <summary>
        /// Asynchronously inserts or updates a denylist entry.
        /// </summary>
        /// <param name="refreshHash">The hash of the refresh token to upsert. Must not be null or empty.</param>
        /// <param name="reason">The reason for denylisting. Must not be null or empty.</param>
        /// <param name="addedAtUtc">The UTC date and time when the entry was added.</param>
        /// <param name="expiresAtUtc">The UTC date and time when the entry expires.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="refreshHash"/> or <paramref name="reason"/> is null or empty.
        /// </exception>
        Task UpsertAsync(string refreshHash, string reason, DateTime addedAtUtc, DateTime expiresAtUtc);
    }
}