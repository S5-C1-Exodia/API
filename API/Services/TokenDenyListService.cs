using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.Services
{
    /// <summary>
    /// Service for managing token denylist operations, including orchestration and TTL handling.
    /// </summary>
    /// <param name="dao">Data Access Object for denylisted refresh tokens.</param>
    /// <param name="clock">Service for retrieving the current time.</param>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
    public class TokenDenyListService(IDenylistedRefreshDao dao, IClockService clock) : ITokenDenyListService
    {
        private readonly IDenylistedRefreshDao _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        private readonly IClockService _clock = clock ?? throw new ArgumentNullException(nameof(clock));

        /// <summary>
        /// Checks if a refresh token hash is denylisted.
        /// </summary>
        /// <param name="refreshTokenHash">The hash of the refresh token to check.</param>
        /// <returns>A task that resolves to true if the token is denylisted, otherwise false.</returns>
        public Task<bool> IsDeniedAsync(string refreshTokenHash)
        {
            var now = _clock.GetUtcNow();
            return _dao.ExistsAsync(refreshTokenHash, now);
        }

        public Task AddAsync(string refreshTokenHash, string reason, DateTime? expiresAtUtc)
        {
            var now = _clock.GetUtcNow();
            var exp = expiresAtUtc ?? now.AddDays(90);
            return _dao.UpsertAsync(refreshTokenHash, reason ?? "logout", now, exp);
        }
    }
}