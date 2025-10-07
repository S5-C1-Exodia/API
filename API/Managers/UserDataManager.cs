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
        public async Task<PlaylistPageDto> GetPlaylistsAsync(string sessionId, string? pageToken,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

            DateTime nowUtc = _clock.GetUtcNow();

            // Normalize page token for DB keys (first page = empty string; never NULL in DB).
            string normalizedPageToken = pageToken ?? string.Empty;

            // 1) Retrieve TokenSet for the session (401 if missing)
            TokenSet? tokenSet = await _tokenDao.GetBySessionAsync(sessionId);
            if (tokenSet is null)
                throw new MissingTokenSetException("No TokenSet associated with the given session.");

            string providerUserId = tokenSet.ProviderUserId;

            // 2) Try to get a valid access token for this session; refresh if absent/expired
            string? accessToken = await _accessTokenDao.GetValidBySessionAsync(sessionId, nowUtc, ct);
            if (string.IsNullOrEmpty(accessToken))
            {
                string refreshToken = tokenSet.RefreshToken;

                RefreshResult refreshed = await _spotifyOAuthHelper.RefreshTokensAsync(refreshToken, ct);
                accessToken = refreshed.AccessToken;

                // Persist short-lived access token
                await _accessTokenDao.UpsertAsync(sessionId, accessToken, refreshed.AccessExpiresAtUtc, nowUtc, ct);

                // If refresh token rotated, update TokenSet; always mirror latest access expiry
                string effectiveRefresh = string.IsNullOrEmpty(refreshed.NewRefreshToken)
                    ? refreshToken
                    : refreshed.NewRefreshToken!;
                await _tokenDao.UpdateAfterRefreshAsync(sessionId, effectiveRefresh, refreshed.AccessExpiresAtUtc, ct);
            }

            // 3) Cache lookup (session + normalized page token)
            string? cachedJson = await _playlistCacheDao.GetPageJsonAsync(sessionId, normalizedPageToken, nowUtc, ct);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                PlaylistPageDto? cachedPage = JsonSerializer.Deserialize<PlaylistPageDto>(cachedJson);
                return cachedPage ?? new PlaylistPageDto();
            }

            // 4) Cache miss → call Spotify (Spotify receives the original pageToken, not normalized)
            PlaylistPageDto livePage = await _spotifyApiHelper.GetPlaylistsAsync(accessToken!, normalizedPageToken, ct);

            // 5) Persist page with TTL (keyed by normalized token)
            int ttlMinutes = _config.GetPlaylistCacheTtlMinutes();
            DateTime expiresAtUtc = nowUtc.AddMinutes(ttlMinutes);
            string rawJson = JsonSerializer.Serialize(livePage);

            await _playlistCacheDao.UpsertPageAsync(
                sessionId,
                providerUserId,
                normalizedPageToken,
                rawJson,
                expiresAtUtc,
                nowUtc,
                ct
            );

            return livePage;
        }
    }
}