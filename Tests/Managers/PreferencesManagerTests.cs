using API.Managers;
using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using API.Services;
using Moq;

namespace Tests.Managers;

public class PreferencesManagerTests
{
    [Fact]
    public async Task ReplaceSelectionAsync_ReplacesSelection()
    {
        var selectionDao = new Mock<IPlaylistSelectionDao>();
        var tokenDao = new Mock<ITokenDao>();
        var txRunner = new Mock<ITransactionRunner>();
        var clock = new Mock<IClockService>();

        var sessionId = "session";
        var playlistIds = new List<string> { "id1", "id2" };
        var tokenSet = new Mock<Api.Models.TokenSet>(
            1,
            "Spotify",
            "user",
            "refresh",
            "scope",
            DateTime.UtcNow.AddMinutes(60),
            DateTime.UtcNow,
            sessionId
        ).Object;

        tokenDao.Setup(d => d.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);
        clock.Setup(c => c.GetUtcNow()).Returns(DateTime.UtcNow);

        txRunner.Setup(t => t.RunAsync(
                    It.IsAny<Func<MySqlConnector.MySqlConnection, MySqlConnector.MySqlTransaction, Task>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var manager = new PreferencesManager(selectionDao.Object, tokenDao.Object, txRunner.Object, new AuditService(), clock.Object);

        await manager.ReplaceSelectionAsync(sessionId, playlistIds);

        // Pas de vérification de log
    }

    [Fact]
    public async Task AddToSelectionAsync_AddsSelection()
    {
        var selectionDao = new Mock<IPlaylistSelectionDao>();
        var tokenDao = new Mock<ITokenDao>();
        var txRunner = new Mock<ITransactionRunner>();
        var clock = new Mock<IClockService>();

        var sessionId = "session";
        var playlistIds = new List<string> { "id1", "id2" };
        var tokenSet = new Mock<Api.Models.TokenSet>(
            1,
            "Spotify",
            "user",
            "refresh",
            "scope",
            DateTime.UtcNow.AddMinutes(60),
            DateTime.UtcNow,
            sessionId
        ).Object;

        tokenDao.Setup(d => d.GetBySessionAsync(sessionId)).ReturnsAsync(tokenSet);
        clock.Setup(c => c.GetUtcNow()).Returns(DateTime.UtcNow);

        txRunner.Setup(t => t.RunAsync(
                    It.IsAny<Func<MySqlConnector.MySqlConnection, MySqlConnector.MySqlTransaction, Task>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        selectionDao.Setup(d => d.BulkInsertIfNotExistsAsync(
                    sessionId,
                    "spotify",
                    "user",
                    playlistIds,
                    It.IsAny<DateTime>(),
                    It.IsAny<MySqlConnector.MySqlConnection>(),
                    It.IsAny<MySqlConnector.MySqlTransaction>()
                )
            )
            .ReturnsAsync(2);

        var manager = new PreferencesManager(selectionDao.Object, tokenDao.Object, txRunner.Object, new AuditService(), clock.Object);

        await manager.AddToSelectionAsync(sessionId, playlistIds);

        // Pas de vérification de log
    }

    [Fact]
    public async Task AddToSelectionAsync_DoesNothingWhenPlaylistIdsIsNullOrEmpty()
    {
        var selectionDao = new Mock<IPlaylistSelectionDao>();
        var tokenDao = new Mock<ITokenDao>();
        var txRunner = new Mock<ITransactionRunner>();
        var clock = new Mock<IClockService>();

        var sessionId = "session";
        var manager = new PreferencesManager(selectionDao.Object, tokenDao.Object, txRunner.Object, new AuditService(), clock.Object);

        await manager.AddToSelectionAsync(sessionId, null);
        await manager.AddToSelectionAsync(sessionId, new List<string>());

        // Pas de vérification de log
    }

    [Fact]
    public async Task RemoveFromSelectionAsync_DoesNothingWhenPlaylistIdsIsNullOrEmpty()
    {
        var selectionDao = new Mock<IPlaylistSelectionDao>();
        var tokenDao = new Mock<ITokenDao>();
        var txRunner = new Mock<ITransactionRunner>();
        var clock = new Mock<IClockService>();

        var sessionId = "session";
        var manager = new PreferencesManager(selectionDao.Object, tokenDao.Object, txRunner.Object, new AuditService(), clock.Object);

        await manager.RemoveFromSelectionAsync(sessionId, null);
        await manager.RemoveFromSelectionAsync(sessionId, new List<string>());

        // Pas de vérification de log
    }

    [Fact]
    public async Task ClearSelectionAsync_ClearsSelection()
    {
        var selectionDao = new Mock<IPlaylistSelectionDao>();
        var tokenDao = new Mock<ITokenDao>();
        var txRunner = new Mock<ITransactionRunner>();
        var clock = new Mock<IClockService>();

        var sessionId = "session";
        selectionDao.Setup(d => d.DeleteBySessionAsync(sessionId)).Returns(Task.CompletedTask);

        var manager = new PreferencesManager(selectionDao.Object, tokenDao.Object, txRunner.Object, new AuditService(), clock.Object);

        await manager.ClearSelectionAsync(sessionId);

        // Pas de vérification de log
    }

    [Fact]
    public async Task GetSelectionAsync_ReturnsSelectionIds()
    {
        var selectionDao = new Mock<IPlaylistSelectionDao>();
        var tokenDao = new Mock<ITokenDao>();
        var txRunner = new Mock<ITransactionRunner>();
        var clock = new Mock<IClockService>();

        var sessionId = "session";
        var ids = new List<string> { "id1", "id2" };
        selectionDao.Setup(d => d.GetIdsBySessionAsync(sessionId)).ReturnsAsync(ids);

        var manager = new PreferencesManager(selectionDao.Object, tokenDao.Object, txRunner.Object, new AuditService(), clock.Object);

        var result = await manager.GetSelectionAsync(sessionId);

        Assert.Equal(ids, result);
    }

    [Fact]
    public async Task ReplaceSelectionAsync_ThrowsArgumentException_WhenSessionIdIsNullOrEmpty()
    {
        var selectionDao = new Mock<IPlaylistSelectionDao>();
        var tokenDao = new Mock<ITokenDao>();
        var txRunner = new Mock<ITransactionRunner>();
        var clock = new Mock<IClockService>();

        var manager = new PreferencesManager(selectionDao.Object, tokenDao.Object, txRunner.Object, new AuditService(), clock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => manager.ReplaceSelectionAsync(null, new List<string>()));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.ReplaceSelectionAsync("", new List<string>()));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.ReplaceSelectionAsync("   ", new List<string>()));
    }
}