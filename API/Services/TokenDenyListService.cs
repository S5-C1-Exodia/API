using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.Services;

/// <inheritdoc />
public class TokenDenyListService(IDenylistedRefreshDao dao, IClockService clock) : ITokenDenyListService
{
    private readonly IDenylistedRefreshDao _dao = dao ?? throw new ArgumentNullException(nameof(dao));
    private readonly IClockService _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public Task<bool> IsDeniedAsync(string refreshTokenHash)
    {
        var now = _clock.GetUtcNow();
        return _dao.ExistsAsync(refreshTokenHash, now);
    }

    /// <inheritdoc />
    public Task AddAsync(string refreshTokenHash, string reason, DateTime? expiresAtUtc)
    {
        var now = _clock.GetUtcNow();
        var exp = expiresAtUtc ?? now.AddDays(90);
        return _dao.UpsertAsync(refreshTokenHash, reason ?? "logout", now, exp);
    }
}