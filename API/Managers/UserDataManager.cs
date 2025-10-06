using System.Text.Json;
using API.Controllers.InterfacesManagers;
using API.DTO;
using API.Errors.Exceptions;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;
using Api.Models;

namespace API.Managers
{
    /// <summary>
    /// Implements the playlist listing flow with DB cache and token refresh.
    /// Pure orchestration: no SQL (DAOs only) and no HTTP (Helpers only).
    /// </summary>
    public sealed class UserDataManager : IUserDataManager
    {
        private readonly ITokenDao _tokenDao;
        private readonly IAccessTokenDao _accessTokenDao;
        private readonly IPlaylistCacheDao _playlistCacheDao;
        private readonly ISpotifyOAuthHelper _spotifyOAuthHelper;
        private readonly ISpotifyApiHelper _spotifyApiHelper;
        private readonly IClockService _clock;
        private readonly IConfigService _config;

        public UserDataManager(
            ITokenDao tokenDao,
            IAccessTokenDao accessTokenDao,
            IPlaylistCacheDao playlistCacheDao,
            ISpotifyOAuthHelper spotifyOAuthHelper,
            ISpotifyApiHelper spotifyApiHelper,
            IClockService clock,
            IConfigService config)
        {
            _tokenDao = tokenDao ?? throw new ArgumentNullException(nameof(tokenDao));
            _accessTokenDao = accessTokenDao ?? throw new ArgumentNullException(nameof(accessTokenDao));
            _playlistCacheDao = playlistCacheDao ?? throw new ArgumentNullException(nameof(playlistCacheDao));
            _spotifyOAuthHelper = spotifyOAuthHelper ?? throw new ArgumentNullException(nameof(spotifyOAuthHelper));
            _spotifyApiHelper = spotifyApiHelper ?? throw new ArgumentNullException(nameof(spotifyApiHelper));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <inheritdoc />
        public async Task<PlaylistPageDto> GetPlaylistsAsync(string sessionId, string? pageToken, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

            DateTime nowUtc = _clock.GetUtcNow();

            // 1) Retrieve TokenSet for the session (401 if missing)
            // NOTE: your current TokenDao.GetBySessionAsync signature doesn't take a CancellationToken.
            TokenSet? tokenSet = await _tokenDao.GetBySessionAsync(sessionId);
            if (tokenSet is null)
                throw new MissingTokenSetException("No TokenSet associated with the given session.");

            string providerUserId = tokenSet.ProviderUserId;

            // 2) Get a valid access token for this session or refresh if absent/expired
            string? accessToken = await _accessTokenDao.GetValidBySessionAsync(sessionId, nowUtc, ct);
            if (string.IsNullOrEmpty(accessToken))
            {
                // refresh token is stored in plaintext for now (per your note)
                string refreshToken = tokenSet.RefreshToken;

                // Refresh flow: returns new access token (+ optional rotated refresh token) and expiry
                RefreshResult refreshed = await _spotifyOAuthHelper.RefreshTokensAsync(refreshToken, ct);
                accessToken = refreshed.AccessToken;

                // Persist access token in DB (short-lived)
                await _accessTokenDao.UpsertAsync(sessionId, accessToken, refreshed.AccessExpiresAtUtc, nowUtc, ct);

                // If provider rotated refresh token, update TokenSet mirror; else, at least update the access expiry
                string effectiveRefreshToken = string.IsNullOrEmpty(refreshed.NewRefreshToken)
                    ? refreshToken
                    : refreshed.NewRefreshToken!;
                await _tokenDao.UpdateAfterRefreshAsync(sessionId, effectiveRefreshToken, refreshed.AccessExpiresAtUtc, ct);
            }

            // 3) Try cache for this session/pageToken
            string? cachedJson = await _playlistCacheDao.GetPageJsonAsync(sessionId, pageToken, nowUtc, ct);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                PlaylistPageDto cachedPage = JsonSerializer.Deserialize<PlaylistPageDto>(cachedJson)
                                             ?? new PlaylistPageDto();
                return cachedPage;
            }

            // 4) Cache miss => call Spotify
            PlaylistPageDto livePage = await _spotifyApiHelper.GetPlaylistsAsync(accessToken!, pageToken, ct);

            // 5) Persist page with TTL
            int ttlMinutes = _config.GetPlaylistCacheTtlMinutes();
            DateTime expiresAtUtc = nowUtc.AddMinutes(ttlMinutes);
            string rawJson = JsonSerializer.Serialize(livePage);

            await _playlistCacheDao.UpsertPageAsync(
                sessionId,
                providerUserId,
                pageToken,
                rawJson,
                expiresAtUtc,
                nowUtc,
                ct);

            return livePage;
        }
    }
}
