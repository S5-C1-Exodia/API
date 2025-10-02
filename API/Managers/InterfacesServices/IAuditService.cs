namespace API.Managers.InterfacesServices;

public interface IAuditService
{
    void LogAuth(string provider, string action, string details);
}