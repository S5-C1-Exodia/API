namespace API.Managers.InterfacesServices;

/// <summary>
/// Interface for audit logging of authentication actions.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an authentication-related action.
    /// </summary>
    /// <param name="provider">The authentication provider name.</param>
    /// <param name="action">The action performed (e.g., "start", "callback").</param>
    /// <param name="details">Additional details about the action.</param>
    void LogAuth(string provider, string action, string details);
    
    /// <summary>
    /// Logs about a user playlist preference change.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="action">The action performed (e.g., "add", "remove", "clear", "replace").</param>
    /// <param name="playlistIds">Additional details about the action.</param>
    void Log(string sessionId, string action, string playlistIds);
}