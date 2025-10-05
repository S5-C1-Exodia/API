using Api.Models;
using MySqlConnector;

namespace Api.Managers.InterfacesServices;

    /// <summary>
    /// Provides methods to manage application sessions.
    /// </summary>
public interface ISessionService
{
    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="deviceInfo">Information about the device.</param>
    /// <param name="nowUtc">The current UTC date and time.</param>
    /// <param name="expiresAt">The UTC expiration date and time.</param>
    /// <returns>The new session ID.</returns>
    Task<string> CreateSessionAsync(string deviceInfo, DateTime nowUtc, DateTime expiresAt);

    /// <summary>
    /// Gets a session by its identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The <see cref="AppSession"/> if found; otherwise, null.</returns>
    Task<AppSession> GetSessionAsync(string sessionId);
    
    Task DeleteAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);

    // ➕ Méthode “haut niveau” (ouvre sa propre connexion) – utile hors transactions globales
    Task DeleteAsync(string sessionId);
}