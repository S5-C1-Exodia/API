using API.Managers;
using API.DTO;
using API.Errors.Exceptions;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;
using Api.Models;
using Moq;

namespace Tests.Managers
{
    public class UserDataManagerTest
    {
        private readonly Mock<ITokenDao> _tokenDao = new Mock<ITokenDao>();
        private readonly Mock<IAccessTokenDao> _accessTokenDao = new Mock<IAccessTokenDao>();
        private readonly Mock<IPlaylistCacheDao> _playlistCacheDao = new Mock<IPlaylistCacheDao>();
        private readonly Mock<ISpotifyOAuthHelper> _spotifyOAuthHelper = new Mock<ISpotifyOAuthHelper>();
        private readonly Mock<ISpotifyApiHelper> _spotifyApiHelper = new Mock<ISpotifyApiHelper>();
        private readonly Mock<IClockService> _clock = new Mock<IClockService>();
        private readonly Mock<IConfigService> _config = new Mock<IConfigService>();

        private UserDataManager CreateManager()
        {
            return new UserDataManager(
                _tokenDao.Object,
                _accessTokenDao.Object,
                _playlistCacheDao.Object,
                _spotifyOAuthHelper.Object,
                _spotifyApiHelper.Object,
                _clock.Object,
                _config.Object
            );
        }

        [Fact]
        public async Task GetPlaylistsAsync_ShouldThrow_OnEmptySessionId()
        {
            var mgr = CreateManager();
            await Assert.ThrowsAsync<ArgumentException>(() => mgr.GetPlaylistsAsync("", null));
        }

        [Fact]
        public async Task GetPlaylistsAsync_ShouldThrow_OnMissingTokenSet()
        {
            _tokenDao.Setup(t => t.GetBySessionAsync("sid")).ReturnsAsync((TokenSet?)null);
            var mgr = CreateManager();
            await Assert.ThrowsAsync<MissingTokenSetException>(() => mgr.GetPlaylistsAsync("sid", null));
        }

        [Fact]
        public async Task GetPlaylistsAsync_ShouldReturnCachedPage_IfCacheHit()
        {
            var now = DateTime.UtcNow;
            var tokenSet = new TokenSet(1, "spotify", "user1", "rt", "scope", now.AddHours(1), now, "sid");
            var cachedDto = new PlaylistPageDto
                { Items = [new PlaylistItemDto { PlaylistId = "pl1" }] };
            var cachedJson = System.Text.Json.JsonSerializer.Serialize(cachedDto);

            _clock.Setup(c => c.GetUtcNow()).Returns(now);
            _tokenDao.Setup(t => t.GetBySessionAsync("sid")).ReturnsAsync(tokenSet);
            _accessTokenDao.Setup(a => a.GetValidBySessionAsync("sid", now, CancellationToken.None))
                .ReturnsAsync("access-token");
            _playlistCacheDao.Setup(p => p.GetPageJsonAsync("sid", null, now, CancellationToken.None)).ReturnsAsync(cachedJson);

            var mgr = CreateManager();
            var result = await mgr.GetPlaylistsAsync("sid", null);

            Assert.Single(result.Items);
            Assert.Equal("pl1", result.Items[0].PlaylistId);
            _spotifyApiHelper.Verify(
                a => a.GetPlaylistsAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
                Times.Never
            );
        }

        [Fact]
        public async Task GetPlaylistsAsync_ShouldRefreshToken_IfNoValidAccessToken()
        {
            var now = DateTime.UtcNow;
            var tokenSet = new TokenSet(1, "spotify", "user1", "rt", "scope", now.AddHours(1), now, "sid");
            var refreshed = new RefreshResult("new-access", now.AddHours(1), "new-refresh");
            var liveDto = new PlaylistPageDto
                { Items = [new PlaylistItemDto { PlaylistId = "pl2" }] };

            _clock.Setup(c => c.GetUtcNow()).Returns(now);
            _tokenDao.Setup(t => t.GetBySessionAsync("sid")).ReturnsAsync(tokenSet);
            _accessTokenDao.Setup(a => a.GetValidBySessionAsync("sid", now, CancellationToken.None))
                .ReturnsAsync((string?)null);
            _spotifyOAuthHelper.Setup(s => s.RefreshTokensAsync("rt", CancellationToken.None)).ReturnsAsync(refreshed);
            _accessTokenDao.Setup(a => a.UpsertAsync(
                        "sid",
                        "new-access",
                        refreshed.AccessExpiresAtUtc,
                        now,
                        CancellationToken.None
                    )
                )
                .Returns(Task.CompletedTask);
            _tokenDao.Setup(t => t.UpdateAfterRefreshAsync(
                        "sid",
                        "new-refresh",
                        refreshed.AccessExpiresAtUtc,
                        CancellationToken.None
                    )
                )
                .Returns(Task.CompletedTask);
            _playlistCacheDao.Setup(p => p.GetPageJsonAsync("sid", null, now, CancellationToken.None))
                .ReturnsAsync((string?)null);
            _spotifyApiHelper.Setup(a => a.GetPlaylistsAsync("new-access", null, CancellationToken.None)).ReturnsAsync(liveDto);
            _config.Setup(c => c.GetPlaylistCacheTtlMinutes()).Returns(10);
            _playlistCacheDao.Setup(p => p.UpsertPageAsync(
                    "sid",
                    "user1",
                    null,
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    now,
                    CancellationToken.None
                )
            ).Returns(Task.CompletedTask);

            var mgr = CreateManager();
            var result = await mgr.GetPlaylistsAsync("sid", null);

            Assert.Single(result.Items);
            Assert.Equal("pl2", result.Items[0].PlaylistId);
            _accessTokenDao.Verify(
                a => a.UpsertAsync("sid", "new-access", refreshed.AccessExpiresAtUtc, now, CancellationToken.None),
                Times.Once
            );
            _tokenDao.Verify(
                t => t.UpdateAfterRefreshAsync("sid", "new-refresh", refreshed.AccessExpiresAtUtc, CancellationToken.None),
                Times.Once
            );
        }

        [Fact]
        public async Task GetPlaylistsAsync_ShouldCallSpotify_AndCache_IfCacheMiss()
        {
            var now = DateTime.UtcNow;
            var tokenSet = new TokenSet(1, "spotify", "user1", "rt", "scope", now.AddHours(1), now, "sid");
            var liveDto = new PlaylistPageDto
                { Items = [new PlaylistItemDto { PlaylistId = "pl3" }] };

            _clock.Setup(c => c.GetUtcNow()).Returns(now);
            _tokenDao.Setup(t => t.GetBySessionAsync("sid")).ReturnsAsync(tokenSet);
            _accessTokenDao.Setup(a => a.GetValidBySessionAsync("sid", now, CancellationToken.None))
                .ReturnsAsync("access-token");
            _playlistCacheDao.Setup(p => p.GetPageJsonAsync("sid", null, now, CancellationToken.None))
                .ReturnsAsync((string?)null);
            _spotifyApiHelper.Setup(a => a.GetPlaylistsAsync("access-token", null, CancellationToken.None))
                .ReturnsAsync(liveDto);
            _config.Setup(c => c.GetPlaylistCacheTtlMinutes()).Returns(10);
            _playlistCacheDao.Setup(p => p.UpsertPageAsync(
                    "sid",
                    "user1",
                    null,
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    now,
                    CancellationToken.None
                )
            ).Returns(Task.CompletedTask);

            var mgr = CreateManager();
            var result = await mgr.GetPlaylistsAsync("sid", null);

            Assert.Single(result.Items);
            Assert.Equal("pl3", result.Items[0].PlaylistId);
            _playlistCacheDao.Verify(
                p => p.UpsertPageAsync(
                    "sid",
                    "user1",
                    null,
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    now,
                    CancellationToken.None
                ),
                Times.Once
            );
        }
    }
}