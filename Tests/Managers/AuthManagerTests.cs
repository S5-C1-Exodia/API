using API.DTO;
using API.Errors;
using API.Managers;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using Api.Managers.InterfacesServices;
using API.Managers.InterfacesServices;
using Api.Models;
using Moq;

namespace Tests.Managers
{
    /// <summary>
    /// Unit tests for <see cref="AuthManager"/>.
    /// </summary>
    public class AuthManagerTests
    {
        private readonly Mock<ICryptoHelper> _crypto;
        private readonly Mock<IUrlBuilderHelper> _urlBuilder;
        private readonly Mock<IPkceDao> _pkceDao;
        private readonly Mock<ISpotifyOAuthHelper> _oauth;
        private readonly Mock<ITokenDao> _tokenDao;
        private readonly Mock<ISessionService> _session;
        private readonly Mock<IDeeplinkHelper> _deeplink;
        private readonly Mock<IClockService> _clock;
        private readonly Mock<IConfigService> _config;

        public AuthManagerTests()
        {
            _crypto = new Mock<ICryptoHelper>();
            _urlBuilder = new Mock<IUrlBuilderHelper>();
            _pkceDao = new Mock<IPkceDao>();
            _oauth = new Mock<ISpotifyOAuthHelper>();
            _tokenDao = new Mock<ITokenDao>();
            _session = new Mock<ISessionService>();
            _deeplink = new Mock<IDeeplinkHelper>();
            _clock = new Mock<IClockService>();
            _config = new Mock<IConfigService>();

            // Config par défaut
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
        }

        /// <summary>
        /// Delegate used to mock the GeneratePkce method.
        /// </summary>
        /// <param name="verifier">The generated code verifier (output).</param>
        /// <param name="challenge">The generated code challenge (output).</param>
        private delegate void GeneratePkceCallback(out string verifier, out string challenge);

        /// <summary>
        /// Tests that <see cref="AuthManager.StartAuthAsync(System.Collections.Generic.IList{string})"/>
        /// generates a state, saves the PKCE entry, and returns the correct URL.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
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

            AuthManager mgr = new AuthManager(
                _crypto.Object,
                _urlBuilder.Object,
                _pkceDao.Object,
                _oauth.Object,
                _tokenDao.Object,
                _session.Object,
                _deeplink.Object,
                _clock.Object,
                _config.Object
            );

            List<string> scopes = new List<string> { "user-read-email" };

            AuthStartResponseDto res = await mgr.StartAuthAsync(scopes);

            Assert.Equal("teststate", res.State);
            Assert.Contains("spotify.com", res.AuthorizationUrl);
            _pkceDao.Verify(p => p.SaveAsync(It.Is<PkceEntry>(e => e.State == "teststate")), Times.Once);
        }

        /// <summary>
        /// Tests that <see cref="AuthManager.StartAuthAsync(System.Collections.Generic.IList{string})"/>
        /// throws an <see cref="ArgumentException"/> if scopes are empty.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when scopes are empty.</exception>
        [Fact]
        public async Task StartAuthAsync_ShouldThrowIfScopesEmpty()
        {
            AuthManager mgr = new AuthManager(
                _crypto.Object,
                _urlBuilder.Object,
                _pkceDao.Object,
                _oauth.Object,
                _tokenDao.Object,
                _session.Object,
                _deeplink.Object,
                _clock.Object,
                _config.Object
            );

            await Assert.ThrowsAsync<ArgumentException>(() => mgr.StartAuthAsync(new List<string>()));
        }

        /// <summary>
        /// Tests that <see cref="AuthManager.HandleCallbackAsync(string, string, string)"/>
        /// throws <see cref="InvalidStateException"/> when the PKCE state is not found.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="InvalidStateException">Thrown when PKCE state is not found.</exception>
        [Fact]
        public async Task HandleCallbackAsync_ShouldThrow_InvalidState_WhenNotFound()
        {
            _pkceDao.Setup(p => p.GetAsync("unknown")).ReturnsAsync((PkceEntry)null);

            AuthManager mgr = new AuthManager(
                _crypto.Object,
                _urlBuilder.Object,
                _pkceDao.Object,
                _oauth.Object,
                _tokenDao.Object,
                _session.Object,
                _deeplink.Object,
                _clock.Object,
                _config.Object
            );

            await Assert.ThrowsAsync<InvalidStateException>(() => mgr.HandleCallbackAsync("code123", "unknown", "deviceX"));
        }

        /// <summary>
        /// Tests that <see cref="AuthManager.HandleCallbackAsync(string, string, string)"/>
        /// throws <see cref="InvalidStateException"/> when the PKCE state is expired.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="InvalidStateException">Thrown when PKCE state is expired.</exception>
        [Fact]
        public async Task HandleCallbackAsync_ShouldThrow_InvalidState_WhenExpired()
        {
            _pkceDao.Setup(p => p.GetAsync("stateX")).ReturnsAsync(
                new PkceEntry("stateX", "verifX", "chalX", new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc))
            );

            AuthManager mgr = new AuthManager(
                _crypto.Object,
                _urlBuilder.Object,
                _pkceDao.Object,
                _oauth.Object,
                _tokenDao.Object,
                _session.Object,
                _deeplink.Object,
                _clock.Object,
                _config.Object
            );

            await Assert.ThrowsAsync<InvalidStateException>(() => mgr.HandleCallbackAsync("code123", "stateX", "deviceX"));
            _pkceDao.Verify(p => p.DeleteAsync("stateX"), Times.Once);
        }

        /// <summary>
        /// Tests that <see cref="AuthManager.HandleCallbackAsync(string, string, string)"/>
        /// completes the OAuth flow and returns the deeplink.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task HandleCallbackAsync_ShouldCompleteFlow_AndReturnDeeplink()
        {
            _pkceDao.Setup(p => p.GetAsync("goodstate")).ReturnsAsync(
                new PkceEntry(
                    "goodstate",
                    "verifier123",
                    "challenge123",
                    _clock.Object.GetUtcNow().AddMinutes(5)
                )
            );

            TokenInfo tokens = new TokenInfo("at", "rt", _clock.Object.GetUtcNow().AddHours(1), "user123", "scope");
            _oauth.Setup(o => o.ExchangeCodeForTokensAsync("codeok", "https://cb", "verifier123"))
                .ReturnsAsync(tokens);

            _tokenDao.Setup(t => t.SaveByStateAsync("goodstate", "spotify", "scope", "rt", "user123", It.IsAny<DateTime>()))
                .ReturnsAsync(42L);

            _session.Setup(s => s.CreateSessionAsync("deviceX", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync("SID123");

            _deeplink.Setup(d => d.BuildDeepLink("SID123")).Returns("swipez://oauth-callback/spotify?sid=SID123");

            AuthManager mgr = new AuthManager(
                _crypto.Object,
                _urlBuilder.Object,
                _pkceDao.Object,
                _oauth.Object,
                _tokenDao.Object,
                _session.Object,
                _deeplink.Object,
                _clock.Object,
                _config.Object
            );

            string deeplink = await mgr.HandleCallbackAsync("codeok", "goodstate", "deviceX");

            Assert.Equal("swipez://oauth-callback/spotify?sid=SID123", deeplink);

            _tokenDao.Verify(
                t => t.SaveByStateAsync("goodstate", "spotify", "scope", "rt", "user123", It.IsAny<DateTime>()),
                Times.Once
            );
            _session.Verify(s => s.CreateSessionAsync("deviceX", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            _pkceDao.Verify(p => p.DeleteAsync("goodstate"), Times.Once);
        }
    }
}