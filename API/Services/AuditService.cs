using API.Managers.InterfacesServices;

namespace API.Services;

/// <summary>
/// Service for audit logging of authentication actions.
/// </summary>
public class AuditService : IAuditService
{
    public void LogAuth(string provider, string action, string details)
    {
        string p = provider ?? string.Empty;
        string a = action ?? string.Empty;
        string d = details ?? string.Empty;
        Console.WriteLine("[AUDIT] provider=" + p + " action=" + a + " details=" + d);
    }
    
    public void Log(string sessionId, string action, string playlistIds)
    {
        string s = sessionId ?? string.Empty;
        string a = action ?? string.Empty;
        string p = playlistIds ?? string.Empty;
        Console.WriteLine("[AUDIT] sessionId=" + s + " action=" + a + " details=" + p);
    }
}