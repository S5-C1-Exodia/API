using API.Managers.InterfacesServices;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesServices;
using Api.Models;
using MySqlConnector;

namespace API.Services
{
    /// <summary>
    /// Service for managing user sessions, including creation and database persistence.
    /// </summary>
    /// <param name="sessionDao">Data Access Object for session-related database operations.</param>
    /// <param name="ids">Service for generating unique session IDs.</param>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
    public class SessionService(ISessionDao sessionDao, IIdGenerator ids) : ISessionService
    {
        private readonly ISessionDao _sessionDao = sessionDao ?? throw new ArgumentNullException(nameof(sessionDao));
        private readonly IIdGenerator _ids = ids ?? throw new ArgumentNullException(nameof(ids));

        /// <summary>
        /// Creates a new user session and persists it to the database.
        /// </summary>
        /// <param name="deviceInfo">Optional information about the user's device.</param>
        /// <param name="createdAtUtc">The UTC timestamp when the session was created.</param>
        /// <param name="expiresAtUtc">The UTC timestamp when the session will expire.</param>
        /// <returns>A task that resolves to the unique ID of the created session.</returns>
        /// <exception cref="Exception">Propagates exceptions thrown during database operations.</exception>
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