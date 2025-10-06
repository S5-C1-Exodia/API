using API.Managers.InterfacesServices;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesServices;
using Api.Models;
using MySqlConnector;

namespace API.Services
{
    /// <inheritdoc />
    public class SessionService(ISessionDao sessionDao, IIdGenerator ids) : ISessionService
    {
        private readonly ISessionDao _sessionDao = sessionDao ?? throw new ArgumentNullException(nameof(sessionDao));
        private readonly IIdGenerator _ids = ids ?? throw new ArgumentNullException(nameof(ids));

        /// <inheritdoc />
        public async Task<string> CreateSessionAsync(string? deviceInfo, DateTime createdAtUtc, DateTime expiresAtUtc)
        {
            string id = _ids.NewSessionId();
            var s = new AppSession(id, deviceInfo ?? string.Empty, createdAtUtc, createdAtUtc, expiresAtUtc);
            await _sessionDao.InsertAsync(s);
            return id;
        }

        /// <inheritdoc />
        public Task<AppSession?> GetSessionAsync(string sessionId) => _sessionDao.GetAsync(sessionId);

        /// <inheritdoc />
        public Task DeleteAsync(string sessionId) => _sessionDao.DeleteAsync(sessionId);

        /// <inheritdoc />
        public Task DeleteAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx)
            => _sessionDao.DeleteAsync(sessionId, conn, tx);
    }
}