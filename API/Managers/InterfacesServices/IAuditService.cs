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
}