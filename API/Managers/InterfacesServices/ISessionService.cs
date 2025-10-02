namespace Api.Services
{
    using System.Threading.Tasks;
    using Api.Models;

    public interface ISessionService
    {
        Task<string> CreateSessionAsync(string deviceInfo, System.DateTime nowUtc, System.DateTime expiresAt);
        Task<AppSession> GetSessionAsync(string sessionId);
    }
}