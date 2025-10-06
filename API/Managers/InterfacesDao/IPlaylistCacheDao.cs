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

    /// <summary>
    /// Returns the cached page JSON for a given session and page token if it is still valid.
    /// The session linkage is checked via <c>playlistcache_session</c>.
    /// Returns <c>null</c> when not found or expired.
    /// </summary>
    /// <param name="sessionId">Opaque application session identifier.</param>
    /// <param name="pageToken">Opaque page token (null/empty for first page).</param>
    /// <param name="nowUtc">Current UTC timestamp (provided by ClockService).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string?> GetPageJsonAsync(string sessionId, string? pageToken, DateTime nowUtc, CancellationToken ct = default);

    /// <summary>
    /// Upserts a cached page and (idempotently) links it to the given session.
    /// If the page already exists, it updates JSON and expiration;
    /// the session link is also upserted.
    /// </summary>
    /// <param name="sessionId">Opaque application session identifier.</param>
    /// <param name="providerUserId">Provider-level user id (e.g., Spotify user id).</param>
    /// <param name="pageToken">Opaque page token (null/empty for first page).</param>
    /// <param name="pageJson">Raw JSON of the playlist page as returned by Spotify (or mapped).</param>
    /// <param name="expiresAtUtc">Expiration timestamp (UTC) for this cache entry.</param>
    /// <param name="nowUtc">Current UTC timestamp (for audit columns).</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpsertPageAsync(
        string sessionId,
        string providerUserId,
        string? pageToken,
        string pageJson,
        DateTime expiresAtUtc,
        DateTime nowUtc,
        CancellationToken ct = default);
}