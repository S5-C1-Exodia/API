using Api.Models;

namespace Api.Managers.InterfacesServices;

public interface ISessionService
{
    Task<string> CreateSessionAsync(string deviceInfo, DateTime nowUtc, DateTime expiresAt);
    Task<AppSession> GetSessionAsync(string sessionId);
}