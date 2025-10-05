using API.DTO;
using API.Errors;
using API.Managers;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using Api.Managers.InterfacesServices;
using API.Managers.InterfacesServices;
using Api.Models;
using Moq;
using MySqlConnector;

namespace Tests.Managers
{
    public class AuthManagerTests
    {
        private readonly Mock<ICryptoHelper> _crypto = new();
        private readonly Mock<IUrlBuilderHelper> _urlBuilder = new();
        private readonly Mock<IPkceDao> _pkceDao = new();
        private readonly Mock<ISpotifyOAuthHelper> _oauth = new();
        private readonly Mock<ITokenDao> _tokenDao = new();
        private readonly Mock<ISessionService> _session = new();
        private readonly Mock<IDeeplinkHelper> _deeplink = new();
        private readonly Mock<IClockService> _clock = new();
        private readonly Mock<IConfigService> _config = new();
        private readonly Mock<ITokenDenyListService> _denylist = new();
        private readonly Mock<IHashService> _hash = new();
        private readonly Mock<ITransactionRunner> _txRunner = new();
        private readonly Mock<IAuditService> _audit = new();

        private readonly Mock<IAccessTokenDao> _accessTokenDao = new();
        private readonly Mock<IPlaylistSelectionDao> _playlistSelectionDao = new();
        private readonly Mock<IPlaylistCacheDao> _playlistCacheDao = new();
        private readonly Mock<IUserProfileCacheDao> _userProfileCacheDao = new();

        public AuthManagerTests()
        {
            _crypto.Setup(c => c.GenerateState(32)).Returns("teststate");
            _crypto.Setup(c => c.GeneratePkce(out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
                .Callback(
                    new GeneratePkceCallback((out string v, out string c) =>
                        {
                            v = "verifier123";
                            c = "challenge123";
                        }
                    )
                );

            _clock.Setup(c => c.GetUtcNow()).Returns(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));
            _config.Setup(c => c.GetPkceTtlMinutes()).Returns(10);
            _config.Setup(c => c.GetSessionTtlMinutes()).Returns(60);
            _config.Setup(c => c.GetSpotifyClientId()).Returns("client123");
            _config.Setup(c => c.GetSpotifyRedirectUri()).Returns("https://cb");

            // Le runner appelle juste le delegate sans vraie connexion/transaction.
            _txRunner
                .Setup(r => r.RunInTransaction(It.IsAny<Func<MySqlConnection, MySqlTransaction, Task>>()))
                .Returns<Func<MySqlConnection, MySqlTransaction, Task>>(f => f(null!, null!));
        }

        private delegate void GeneratePkceCallback(out string verifier, out string challenge);

        private AuthManager CreateManager()
        {
            return new AuthManager(
                _crypto.Object,
                _urlBuilder.Object,
                _pkceDao.Object,
                _oauth.Object,
                _tokenDao.Object,
                _session.Object,
                _deeplink.Object,
                _clock.Object,
                _config.Object,
                _accessTokenDao.Object,
                _playlistSelectionDao.Object,
                _playlistCacheDao.Object,
                _userProfileCacheDao.Object,
                _denylist.Object,
                _hash.Object,
                _audit.Object,
                _txRunner.Object
            );
        }

        [Fact]
        public async Task StartAuthAsync_ShouldGenerateState_AndSavePkce_AndReturnUrl()
        {
            _urlBuilder
                .Setup(u => u.BuildAuthorizeUrl(
                        "client123",
                        "https://cb",
                        It.IsAny<string[]>(),
                        "teststate",
                        "challenge123",
                        "S256"
                    )
                )
                .Returns("https://spotify.com/auth?state=teststate");

            var mgr = CreateManager();

            var scopes = new List<string> { "user-read-email" };
            var res = await mgr.StartAuthAsync(scopes);

            Assert.Equal("teststate", res.State);
            Assert.Contains("spotify.com", res.AuthorizationUrl);
            _pkceDao.Verify(p => p.SaveAsync(It.Is<PkceEntry>(e => e.State == "teststate")), Times.Once);
        }

        [Fact]
        public async Task StartAuthAsync_ShouldThrowIfScopesEmpty()
        {
            var mgr = CreateManager();
            await Assert.ThrowsAsync<ArgumentException>(() => mgr.StartAuthAsync(new List<string>()));
        }

        [Fact]
        public async Task HandleCallbackAsync_ShouldThrow_InvalidState_WhenNotFound()
        {
            _pkceDao.Setup(p => p.GetAsync("unknown"))!.ReturnsAsync((PkceEntry)null!);

            var mgr = CreateManager();

            await Assert.ThrowsAsync<InvalidStateException>(() =>
                mgr.HandleCallbackAsync("code123", "unknown", "deviceX")
            );
        }

        [Fact]
        public async Task HandleCallbackAsync_ShouldThrow_InvalidState_WhenExpired()
        {
            _pkceDao.Setup(p => p.GetAsync("stateX"))
                .ReturnsAsync(
                    new PkceEntry(
                        "stateX",
                        "v",
                        "c",
                        new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                    )
                );

            var mgr = CreateManager();

            await Assert.ThrowsAsync<InvalidStateException>(() =>
                mgr.HandleCallbackAsync("code123", "stateX", "deviceX")
            );

            _pkceDao.Verify(p => p.DeleteAsync("stateX"), Times.Once);
        }

        [Fact]
        public async Task HandleCallbackAsync_ShouldCompleteFlow_AndReturnDeeplink()
        {
            _pkceDao.Setup(p => p.GetAsync("goodstate"))
                .ReturnsAsync(
                    new PkceEntry(
                        "goodstate",
                        "verifier123",
                        "challenge123",
                        _clock.Object.GetUtcNow().AddMinutes(5)
                    )
                );

            var tokens = new TokenInfo(
                "at",
                "rt",
                _clock.Object.GetUtcNow().AddHours(1),
                "user123",
                "scope"
            );

            _oauth.Setup(o => o.ExchangeCodeForTokensAsync("codeok", "https://cb", "verifier123"))
                .ReturnsAsync(tokens);

            _tokenDao.Setup(t => t.SaveByStateAsync(
                        "goodstate",
                        "spotify",
                        "scope",
                        "rt",
                        "user123",
                        It.IsAny<DateTime>()
                    )
                )
                .ReturnsAsync(42L);

            _session.Setup(s => s.CreateSessionAsync("deviceX", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync("SID123");

            _deeplink.Setup(d => d.BuildDeepLink("SID123"))
                .Returns("swipez://oauth-callback/spotify?sid=SID123");

            var mgr = CreateManager();

            var deeplink = await mgr.HandleCallbackAsync("codeok", "goodstate", "deviceX");

            Assert.Equal("swipez://oauth-callback/spotify?sid=SID123", deeplink);

            _tokenDao.Verify(
                t => t.SaveByStateAsync(
                    "goodstate",
                    "spotify",
                    "scope",
                    "rt",
                    "user123",
                    It.IsAny<DateTime>()
                ),
                Times.Once
            );
            _session.Verify(s => s.CreateSessionAsync("deviceX", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            _pkceDao.Verify(p => p.DeleteAsync("goodstate"), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldPurgeSessionAndDenylistRefreshToken()
        {
            var sessionId = "session123";
            var now = _clock.Object.GetUtcNow();

            var tokenSet = new TokenSet(
                tokenSetId: 1,
                provider: "spotify",
                providerUserId: "puid-1",
                refreshTokenEnc: "refresh-token-enc",
                scope: "scope",
                accessExpiresAt: now.AddHours(1),
                updatedAt: now,
                sessionId: sessionId
            );
            _tokenDao.Setup(t => t.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);

            _hash.Setup(h => h.Sha256Base64("refresh-token-enc")).Returns("hashed-refresh");

            // Le runner exécute la lambda, où les DAO transactionnels sont appelés
            _accessTokenDao
                .Setup(d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _playlistSelectionDao
                .Setup(d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _playlistCacheDao
                .Setup(d => d.DeleteLinksBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _playlistCacheDao
                .Setup(d => d.DeleteByProviderUserAsync("puid-1", It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _userProfileCacheDao
                .Setup(d => d.DeleteByProviderUserAsync("puid-1", It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _tokenDao.Setup(d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _session.Setup(s => s.DeleteAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);

            var mgr = CreateManager();
            await mgr.LogoutAsync(sessionId);

            _denylist.Verify(d => d.AddAsync("hashed-refresh", "logout", It.IsAny<DateTime>()), Times.Once);

            _accessTokenDao.Verify(
                d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _playlistSelectionDao.Verify(
                d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _playlistCacheDao.Verify(
                d => d.DeleteLinksBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _playlistCacheDao.Verify(
                d => d.DeleteByProviderUserAsync("puid-1", It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _userProfileCacheDao.Verify(
                d => d.DeleteByProviderUserAsync("puid-1", It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _tokenDao.Verify(
                d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _session.Verify(
                s => s.DeleteAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );

            _audit.Verify(a => a.LogAuth("spotify", "SpotifyLogout", "purge DB + denylist"), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldBeIdempotent_WhenTokenSetAbsent()
        {
            var sessionId = "sid-absent";
            _tokenDao.Setup(t => t.GetBySessionAsync(sessionId)).ReturnsAsync((TokenSet?)null);

            _accessTokenDao
                .Setup(d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _playlistSelectionDao
                .Setup(d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _playlistCacheDao
                .Setup(d => d.DeleteLinksBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _tokenDao.Setup(d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);
            _session.Setup(s => s.DeleteAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .Returns(Task.CompletedTask);

            var mgr = CreateManager();
            await mgr.LogoutAsync(sessionId);

            _denylist.Verify(d => d.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);

            _accessTokenDao.Verify(
                d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _playlistSelectionDao.Verify(
                d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _playlistCacheDao.Verify(
                d => d.DeleteLinksBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _tokenDao.Verify(
                d => d.DeleteBySessionAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );
            _session.Verify(
                s => s.DeleteAsync(sessionId, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()),
                Times.Once
            );

            _audit.Verify(a => a.LogAuth("spotify", "SpotifyLogout", "purge DB + denylist"), Times.Once);
        }
    }
}