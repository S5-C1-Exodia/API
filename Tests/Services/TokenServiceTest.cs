using API.DTO;
using API.Errors.Exceptions;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;
using Api.Models;
using API.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class TokenServiceTest
{
    private readonly Mock<ITokenDao> _tokenDaoMock;
        private readonly Mock<IAccessTokenDao> _accessTokenDaoMock;
        private readonly Mock<ISpotifyOAuthHelper> _spotifyOAuthHelperMock;
        private readonly Mock<IClockService> _clockServiceMock;
        private readonly TokenService _tokenService;

        public TokenServiceTest()
        {
            _tokenDaoMock = new Mock<ITokenDao>();
            _accessTokenDaoMock = new Mock<IAccessTokenDao>();
            _spotifyOAuthHelperMock = new Mock<ISpotifyOAuthHelper>();
            _clockServiceMock = new Mock<IClockService>();

            _clockServiceMock.Setup(c => c.GetUtcNow()).Returns(DateTime.UtcNow);

            _tokenService = new TokenService(
                _tokenDaoMock.Object,
                _accessTokenDaoMock.Object,
                _spotifyOAuthHelperMock.Object,
                _clockServiceMock.Object);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ShouldReturnExistingAccessToken_WhenValid()
        {
            // Arrange
            const string sessionId = "session123";
            const string existingAccessToken = "access_abc";
            var now = DateTime.UtcNow;

            var tokenSet = new TokenSet(1, "spotify", "user_1", "refresh_123", "scope", now.AddHours(1), now, sessionId);

            _tokenDaoMock.Setup(t => t.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);
            _accessTokenDaoMock.Setup(a => a.GetValidBySessionAsync(sessionId, now, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAccessToken);

            _clockServiceMock.Setup(c => c.GetUtcNow()).Returns(now);

            // Act
            var result = await _tokenService.GetAccessTokenAsync(sessionId);

            // Assert
            result.Should().Be(existingAccessToken);
            _spotifyOAuthHelperMock.Verify(s => s.RefreshTokensAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _accessTokenDaoMock.Verify(a => a.UpsertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ShouldRefreshToken_WhenAccessTokenInvalid()
        {
            // Arrange
            const string sessionId = "session123";
            const string newAccessToken = "access_new";
            const string refreshToken = "refresh_123";
            const string providerUserId = "user_1";
            var now = DateTime.UtcNow;

            var tokenSet = new TokenSet(1, "spotify", providerUserId, refreshToken, "scope", now.AddHours(1), now, sessionId);

            var refreshed = new RefreshResult(newAccessToken, now.AddHours(1), null);

            _tokenDaoMock.Setup(t => t.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);
            _accessTokenDaoMock.Setup(a => a.GetValidBySessionAsync(sessionId, now, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);
            _spotifyOAuthHelperMock.Setup(s => s.RefreshTokensAsync(refreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshed);
            _clockServiceMock.Setup(c => c.GetUtcNow()).Returns(now);

            // Act
            var result = await _tokenService.GetAccessTokenAsync(sessionId);

            // Assert
            result.Should().Be(newAccessToken);

            _accessTokenDaoMock.Verify(a =>
                a.UpsertAsync(sessionId, newAccessToken, refreshed.AccessExpiresAtUtc, now, It.IsAny<CancellationToken>()),
                Times.Once);

            _tokenDaoMock.Verify(t =>
                t.UpdateAfterRefreshAsync(sessionId, refreshToken, refreshed.AccessExpiresAtUtc, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ShouldUseNewRefreshToken_WhenRotated()
        {
            // Arrange
            const string sessionId = "session123";
            const string refreshToken = "refresh_123";
            const string newRefreshToken = "refresh_rotated";
            const string newAccessToken = "access_new";
            var now = DateTime.UtcNow;

            var tokenSet = new TokenSet(1, "spotify", "user_1", refreshToken, "scope", now.AddHours(1), now, sessionId);

            var refreshed = new RefreshResult(newAccessToken, now.AddMinutes(45), newRefreshToken);

            _tokenDaoMock.Setup(t => t.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);
            _accessTokenDaoMock.Setup(a => a.GetValidBySessionAsync(sessionId, now, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);
            _spotifyOAuthHelperMock.Setup(s => s.RefreshTokensAsync(refreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshed);
            _clockServiceMock.Setup(c => c.GetUtcNow()).Returns(now);

            // Act
            var result = await _tokenService.GetAccessTokenAsync(sessionId);

            // Assert
            result.Should().Be(newAccessToken);

            _tokenDaoMock.Verify(t =>
                t.UpdateAfterRefreshAsync(sessionId, newRefreshToken, refreshed.AccessExpiresAtUtc, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ShouldThrow_WhenTokenSetIsMissing()
        {
            // Arrange
            const string sessionId = "invalid";
            _tokenDaoMock.Setup(t => t.GetBySessionAsync(sessionId)).ReturnsAsync((TokenSet?)null);

            // Act
            Func<Task> act = async () => await _tokenService.GetAccessTokenAsync(sessionId);

            // Assert
            await act.Should().ThrowAsync<MissingTokenSetException>();
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenDependenciesAreNull()
        {
            // Arrange / Act
            Action act1 = () => new TokenService(null!, _accessTokenDaoMock.Object, _spotifyOAuthHelperMock.Object, _clockServiceMock.Object);
            Action act2 = () => new TokenService(_tokenDaoMock.Object, null!, _spotifyOAuthHelperMock.Object, _clockServiceMock.Object);
            Action act3 = () => new TokenService(_tokenDaoMock.Object, _accessTokenDaoMock.Object, null!, _clockServiceMock.Object);
            Action act4 = () => new TokenService(_tokenDaoMock.Object, _accessTokenDaoMock.Object, _spotifyOAuthHelperMock.Object, null!);

            // Assert
            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
            act3.Should().Throw<ArgumentNullException>();
            act4.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task GetAccessTokenAsync_ShouldThrow_WhenSessionIdIsEmpty()
        {
            const string invalidSession = "";

            Func<Task> act = async () => await _tokenService.GetAccessTokenAsync(invalidSession);

            await act.Should().ThrowAsync<ArgumentException>();
        }
}