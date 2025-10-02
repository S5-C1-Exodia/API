using Api.Models;

namespace Api.Managers.InterfacesServices;

public interface ISessionService
{
    Task<string> CreateSessionAsync(string deviceInfo, System.DateTime nowUtc, System.DateTime expiresAt);
    Task<AppSession> GetSessionAsync(string sessionId);
}