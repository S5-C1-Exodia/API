using API.Managers.InterfacesServices;

namespace API.Services;

public class AuditService : IAuditService
{
    public AuditService()
    {
    }

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