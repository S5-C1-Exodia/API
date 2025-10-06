using Api.Models;
using MySqlConnector;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for token data access operations.
/// </summary>
public interface ITokenDao
{
    /// <summary>
    /// Saves a token set by state asynchronously.
    /// </summary>
    /// <param name="state">The OAuth state value.</param>
    /// <param name="provider">The authentication provider name.</param>
    /// <param name="providerUserId">The provider user identifier.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="scope">The granted scopes.</param>
    /// <param name="accessExpiresAt">The access token expiration date and time.</param>
    /// <returns>The ID of the saved token set.</returns>
    Task<long> SaveByStateAsync(string state, string provider, string providerUserId,
        string refreshToken, string scope, DateTime accessExpiresAt);

    /// <summary>
    /// Attaches a token set to a session asynchronously.
    /// </summary>
    /// <param name="tokenSetId">The token set identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    Task AttachToSessionAsync(long tokenSetId, string sessionId);

    /// <summary>
    /// Gets the token set associated with a session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The token set, or null if not found.</returns>
    Task<TokenSet?> GetBySessionAsync(string sessionId);

    /// <summary>
    /// Deletes the token set associated with a session asynchronously.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    Task DeleteBySessionAsync(string sessionId);

    /// <summary>
    /// Deletes the token set associated with a session within a transaction asynchronously.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="conn">The MySQL connection.</param>
    /// <param name="tx">The MySQL transaction.</param>
    Task DeleteBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);

    /// <summary>
    /// Met à jour le refresh token et la date d\'expiration de l\'accès après un flux de rafraîchissement réussi.
    /// </summary>
    /// <param name="sessionId">Identifiant de la session associée au token.</param>
    /// <param name="newRefreshToken">Nouveau refresh token à enregistrer.</param>
    /// <param name="newAccessExpiresAtUtc">Nouvelle date d\'expiration de l\'access token (UTC).</param>
    /// <param name="ct">Jeton d\'annulation pour l\'opération asynchrone (optionnel).</param>
    Task UpdateAfterRefreshAsync(string sessionId, string newRefreshToken, DateTime newAccessExpiresAtUtc,
        CancellationToken ct = default);
}