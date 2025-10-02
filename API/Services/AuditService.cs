using API.Managers.InterfacesServices;

namespace API.Services;

public class AuditService : IAuditService
{
    public AuditService()
    {
    }

    public void LogAuth(string provider, string action, string details)
    {
        // Implémentation minimale : Console. Tu pourras remplacer par MySQL (AUDITLOG) plus tard.
        // Format simple et strict pour la relecture.
        string p = provider ?? string.Empty;
        string a = action ?? string.Empty;
        string d = details ?? string.Empty;
        Console.WriteLine("[AUDIT] provider=" + p + " action=" + a + " details=" + d);
    }
}