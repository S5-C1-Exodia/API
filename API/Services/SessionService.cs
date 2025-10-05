using API.Managers.InterfacesServices;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesServices;
using Api.Models;
using MySqlConnector;

namespace API.Services
{
    public class SessionService(ISessionDao sessionDao, IIdGenerator ids, IClockService clock) : ISessionService
    {
        private readonly ISessionDao _sessionDao = sessionDao ?? throw new ArgumentNullException(nameof(sessionDao));
        private readonly IIdGenerator _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        private readonly IClockService _clock = clock ?? throw new ArgumentNullException(nameof(clock));

        public async Task<string> CreateSessionAsync(string? deviceInfo, DateTime createdAtUtc, DateTime expiresAtUtc)
        {
            string id = _ids.NewSessionId();
            var s = new AppSession(id, deviceInfo ?? string.Empty, createdAtUtc, createdAtUtc, expiresAtUtc);
            await _sessionDao.InsertAsync(s);
            return id;
        }

        public Task<AppSession?> GetSessionAsync(string sessionId) => _sessionDao.GetAsync(sessionId);

        public Task DeleteAsync(string sessionId) => _sessionDao.DeleteAsync(sessionId);

        public Task DeleteAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx)
            => _sessionDao.DeleteAsync(sessionId, conn, tx);
    }
}