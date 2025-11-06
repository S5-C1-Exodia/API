using API.DTO;
using API.Errors.Exceptions;
using API.Managers;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;
using Api.Models;
using Moq;

namespace Tests.Managers;

public class UserDataManagerTests
{
    [Fact]
    public async Task GetPlaylistsAsync_ReturnsCachedPage_WhenCacheExists()
    {
        var tokenDao = new Mock<ITokenDao>();
        var accessTokenDao = new Mock<IAccessTokenDao>();
        var playlistCacheDao = new Mock<IPlaylistCacheDao>();
        var spotifyOAuthHelper = new Mock<ISpotifyOAuthHelper>();
        var spotifyApiHelper = new Mock<ISpotifyApiHelper>();
        var clock = new Mock<IClockService>();
        var config = new Mock<IConfigService>();

        var now = DateTime.UtcNow;
        clock.Setup(c => c.GetUtcNow()).Returns(now);

        var sessionId = "session";
        var tokenSet = new TokenSet(
            1, // tokenSetId
            "Spotify", // provider
            "user", // providerUserId
            "refresh", // refreshToken
            "scope", // scope
            DateTime.UtcNow.AddMinutes(60), // accessExpiresAt
            DateTime.UtcNow, // updatedAt
            sessionId // sessionId
        );
        tokenDao.Setup(d => d.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);
        accessTokenDao.Setup(a => a.GetValidBySessionAsync(sessionId, now, It.IsAny<CancellationToken>()))
            .ReturnsAsync("access");
        playlistCacheDao.Setup(p => p.GetPageJsonAsync(sessionId, "", now, It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Json.JsonSerializer.Serialize(new PlaylistPageDto()));

        var manager = new UserDataManager(
            tokenDao.Object,
            accessTokenDao.Object,
            playlistCacheDao.Object,
            spotifyOAuthHelper.Object,
            spotifyApiHelper.Object,
            clock.Object,
            config.Object
        );

        var result = await manager.GetPlaylistsAsync(sessionId, null);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetPlaylistsAsync_ThrowsArgumentException_WhenSessionIdIsNullOrEmpty()
    {
        var manager = new UserDataManager(
            Mock.Of<ITokenDao>(),
            Mock.Of<IAccessTokenDao>(),
            Mock.Of<IPlaylistCacheDao>(),
            Mock.Of<ISpotifyOAuthHelper>(),
            Mock.Of<ISpotifyApiHelper>(),
            Mock.Of<IClockService>(),
            Mock.Of<IConfigService>()
        );

        await Assert.ThrowsAsync<ArgumentException>(() => manager.GetPlaylistsAsync(null, null));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.GetPlaylistsAsync("", null));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.GetPlaylistsAsync("   ", null));
    }

    [Fact]
    public async Task GetPlaylistsAsync_ThrowsMissingTokenSetException_WhenTokenSetIsMissing()
    {
        var tokenDao = new Mock<ITokenDao>();
        tokenDao.Setup(d => d.GetBySessionAsync(It.IsAny<string>())).ReturnsAsync((TokenSet)null);

        var manager = new UserDataManager(
            tokenDao.Object,
            Mock.Of<IAccessTokenDao>(),
            Mock.Of<IPlaylistCacheDao>(),
            Mock.Of<ISpotifyOAuthHelper>(),
            Mock.Of<ISpotifyApiHelper>(),
            Mock.Of<IClockService>(),
            Mock.Of<IConfigService>()
        );

        await Assert.ThrowsAsync<MissingTokenSetException>(() => manager.GetPlaylistsAsync("session", null));
    }

    [Fact]
    public async Task GetPlaylistsAsync_RefreshesToken_WhenAccessTokenIsMissing()
    {
        var tokenDao = new Mock<ITokenDao>();
        var accessTokenDao = new Mock<IAccessTokenDao>();
        var playlistCacheDao = new Mock<IPlaylistCacheDao>();
        var spotifyOAuthHelper = new Mock<ISpotifyOAuthHelper>();
        var spotifyApiHelper = new Mock<ISpotifyApiHelper>();
        var clock = new Mock<IClockService>();
        var config = new Mock<IConfigService>();

        var now = DateTime.UtcNow;
        clock.Setup(c => c.GetUtcNow()).Returns(now);

        var sessionId = "session";
        var tokenSet = new TokenSet(
            1, // tokenSetId
            "Spotify", // provider
            "user", // providerUserId
            "refresh", // refreshToken
            "scope", // scope
            DateTime.UtcNow.AddMinutes(60), // accessExpiresAt
            DateTime.UtcNow, // updatedAt
            sessionId // sessionId
        );
        tokenDao.Setup(d => d.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);
        accessTokenDao.Setup(a => a.GetValidBySessionAsync(sessionId, now, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        var refreshResult = new RefreshResult(
            "newAccess",
            now.AddMinutes(60),
            "newRefresh"
        );
        spotifyOAuthHelper.Setup(s => s.RefreshTokensAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshResult);

        playlistCacheDao.Setup(p => p.GetPageJsonAsync(sessionId, "", now, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        spotifyApiHelper.Setup(s => s.GetPlaylistsAsync("newAccess", "", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistPageDto());

        config.Setup(c => c.GetPlaylistCacheTtlMinutes()).Returns(10);

        var manager = new UserDataManager(
            tokenDao.Object,
            accessTokenDao.Object,
            playlistCacheDao.Object,
            spotifyOAuthHelper.Object,
            spotifyApiHelper.Object,
            clock.Object,
            config.Object
        );

        var result = await manager.GetPlaylistsAsync(sessionId, null);

        Assert.NotNull(result);
        spotifyOAuthHelper.Verify(s => s.RefreshTokensAsync("refresh", It.IsAny<CancellationToken>()), Times.Once);
        accessTokenDao.Verify(
            a => a.UpsertAsync(sessionId, "newAccess", refreshResult.AccessExpiresAtUtc, now, It.IsAny<CancellationToken>()),
            Times.Once
        );
        tokenDao.Verify(
            t => t.UpdateAfterRefreshAsync(
                sessionId,
                "newRefresh",
                refreshResult.AccessExpiresAtUtc,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetPlaylistsAsync_CallsSpotifyApiAndCachesResult_WhenCacheMiss()
    {
        var tokenDao = new Mock<ITokenDao>();
        var accessTokenDao = new Mock<IAccessTokenDao>();
        var playlistCacheDao = new Mock<IPlaylistCacheDao>();
        var spotifyOAuthHelper = new Mock<ISpotifyOAuthHelper>();
        var spotifyApiHelper = new Mock<ISpotifyApiHelper>();
        var clock = new Mock<IClockService>();
        var config = new Mock<IConfigService>();

        var now = DateTime.UtcNow;
        clock.Setup(c => c.GetUtcNow()).Returns(now);

        var sessionId = "session";
        var tokenSet = new TokenSet(
            1, // tokenSetId
            "Spotify", // provider
            "user", // providerUserId
            "refresh", // refreshToken
            "scope", // scope
            DateTime.UtcNow.AddMinutes(60), // accessExpiresAt
            DateTime.UtcNow, // updatedAt
            sessionId // sessionId
        );
        tokenDao.Setup(d => d.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);
        accessTokenDao.Setup(a => a.GetValidBySessionAsync(sessionId, now, It.IsAny<CancellationToken>()))
            .ReturnsAsync("access");

        playlistCacheDao.Setup(p => p.GetPageJsonAsync(sessionId, "", now, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        var livePage = new PlaylistPageDto();
        spotifyApiHelper.Setup(s => s.GetPlaylistsAsync("access", "", It.IsAny<CancellationToken>()))
            .ReturnsAsync(livePage);

        config.Setup(c => c.GetPlaylistCacheTtlMinutes()).Returns(10);

        var manager = new UserDataManager(
            tokenDao.Object,
            accessTokenDao.Object,
            playlistCacheDao.Object,
            spotifyOAuthHelper.Object,
            spotifyApiHelper.Object,
            clock.Object,
            config.Object
        );

        var result = await manager.GetPlaylistsAsync(sessionId, null);

        Assert.NotNull(result);
        playlistCacheDao.Verify(
            p => p.UpsertPageAsync(
                sessionId,
                tokenSet.ProviderUserId,
                "",
                It.IsAny<string>(),
                now.AddMinutes(10),
                now,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}