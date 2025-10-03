using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesServices;
using API.Managers.InterfacesServices;
using Api.Models;

namespace API.Services;

public class SessionService(ISessionDao dao, IIdGenerator ids) : ISessionService
{
    private readonly ISessionDao _dao = dao ?? throw new ArgumentNullException(nameof(dao));
    private readonly IIdGenerator _ids = ids ?? throw new ArgumentNullException(nameof(ids));

    public async Task<string> CreateSessionAsync(string deviceInfo, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        string sessionId = this._ids.NewSessionId();
        DateTime lastSeen = createdAtUtc;

        AppSession session = new AppSession(sessionId, deviceInfo ?? string.Empty, createdAtUtc, lastSeen, expiresAtUtc);
        await this._dao.InsertAsync(session);

        return sessionId;
    }

    public async Task<AppSession> GetSessionAsync(string sessionId)
    {
        AppSession session = await this._dao.GetAsync(sessionId);
        return session;
    }
}