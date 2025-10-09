using API.DTO;
using API.Errors.Exceptions;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;
using Api.Models;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly ITokenDao _tokenDao;
    private readonly IAccessTokenDao _accessTokenDao;
    private readonly ISpotifyOAuthHelper _spotifyOAuthHelper;
    private readonly IClockService _clock;

    public TokenService(
        ITokenDao tokenDao,
        IAccessTokenDao accessTokenDao,
        ISpotifyOAuthHelper spotifyOAuthHelper,
        IClockService clock)
    {
        _tokenDao = tokenDao ?? throw new ArgumentNullException(nameof(tokenDao));
        _accessTokenDao = accessTokenDao ?? throw new ArgumentNullException(nameof(accessTokenDao));
        _spotifyOAuthHelper = spotifyOAuthHelper ?? throw new ArgumentNullException(nameof(spotifyOAuthHelper));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }
    
    public async Task<string> GetAccessTokenAsync(string sessionId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        DateTime nowUtc = _clock.GetUtcNow();
        TokenSet? tokenSet = await _tokenDao.GetBySessionAsync(sessionId);
        if (tokenSet is null)
            throw new MissingTokenSetException("No TokenSet associated with the given session.");

        string? accessToken = await _accessTokenDao.GetValidBySessionAsync(sessionId, nowUtc, ct);
        if (!string.IsNullOrEmpty(accessToken))
            return accessToken;

        RefreshResult refreshed = await _spotifyOAuthHelper.RefreshTokensAsync(tokenSet.RefreshToken, ct);
        accessToken = refreshed.AccessToken;

        await _accessTokenDao.UpsertAsync(sessionId, accessToken, refreshed.AccessExpiresAtUtc, nowUtc, ct);
        string newRefresh = string.IsNullOrEmpty(refreshed.NewRefreshToken)
            ? tokenSet.RefreshToken
            : refreshed.NewRefreshToken!;
        await _tokenDao.UpdateAfterRefreshAsync(sessionId, newRefresh, refreshed.AccessExpiresAtUtc, ct);

        return accessToken;
    }
}