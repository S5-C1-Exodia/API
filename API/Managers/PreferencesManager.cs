using API.Controllers.InterfacesManagers;
using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;

namespace API.Managers;

/// <summary>
/// Implements playlist preferences operations (replace/add/remove/clear/get) using DAO + transaction runner.
/// </summary>
public sealed class PreferencesManager(
    IPlaylistSelectionDao selectionDao,
    ITokenDao tokenDao,
    ITransactionRunner txRunner,
    IAuditService audit,
    IClockService clock)
    : IPreferencesManager
{
    private readonly IPlaylistSelectionDao _selectionDao =
        selectionDao ?? throw new ArgumentNullException(nameof(selectionDao));

    private readonly ITokenDao _tokenDao = tokenDao ?? throw new ArgumentNullException(nameof(tokenDao));
    private readonly ITransactionRunner _txRunner = txRunner ?? throw new ArgumentNullException(nameof(txRunner));
    private readonly IAuditService _audit = audit ?? throw new ArgumentNullException(nameof(audit));
    private readonly IClockService _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <inheritdoc />
    public async Task ReplaceSelectionAsync(string sessionId, IReadOnlyCollection<string> playlistIds,
        CancellationToken ct = default)
    {
        EnsureSession(sessionId);
        playlistIds ??= Array.Empty<string>();

        var tokenSet = await _tokenDao.GetBySessionAsync(sessionId)
                       ?? throw new InvalidOperationException("No TokenSet for the given session.");
        string provider = "spotify";
        string providerUserId = tokenSet.ProviderUserId ?? throw new InvalidOperationException("Missing ProviderUserId.");
        DateTime now = _clock.GetUtcNow();

        await _txRunner.RunAsync(
            async (conn, tx) =>
            {
                await _selectionDao.DeleteBySessionAsync(sessionId, conn, tx);
                if (playlistIds.Count > 0)
                {
                    await _selectionDao.BulkInsertAsync(sessionId, provider, providerUserId, playlistIds, now, conn, tx);
                }
            },
            ct
        );

        _audit.Log(sessionId, "ReplaceSelection", $"count={playlistIds.Count}");
    }

    /// <inheritdoc />
    public async Task AddToSelectionAsync(string sessionId, IReadOnlyCollection<string> playlistIds,
        CancellationToken ct = default)
    {
        EnsureSession(sessionId);
        if (playlistIds is null || playlistIds.Count == 0)
        {
            _audit.Log(sessionId, "AddToSelection", "added=0");
            return;
        }

        var tokenSet = await _tokenDao.GetBySessionAsync(sessionId)
                       ?? throw new InvalidOperationException("No TokenSet for the given session.");
        string provider = "spotify";
        string providerUserId = tokenSet.ProviderUserId ?? throw new InvalidOperationException("Missing ProviderUserId.");
        DateTime now = _clock.GetUtcNow();

        int inserted = 0;
        await _txRunner.RunAsync(
            async (conn, tx) =>
            {
                inserted = await _selectionDao.BulkInsertIfNotExistsAsync(
                    sessionId,
                    provider,
                    providerUserId,
                    playlistIds,
                    now,
                    conn,
                    tx
                );
            },
            ct
        );

        _audit.Log(sessionId, "AddToSelection", $"added={inserted}");
    }

    /// <inheritdoc />
    public async Task RemoveFromSelectionAsync(string sessionId, IReadOnlyCollection<string> playlistIds,
        CancellationToken ct = default)
    {
        EnsureSession(sessionId);
        if (playlistIds is null || playlistIds.Count == 0)
        {
            _audit.Log(sessionId, "RemoveFromSelection", "removed=0");
            return;
        }

        int removed = 0;
        await _txRunner.RunAsync(
            async (conn, tx) => { removed = await _selectionDao.BulkDeleteByIdsAsync(sessionId, playlistIds, conn, tx); },
            ct
        );

        _audit.Log(sessionId, "RemoveFromSelection", $"removed={removed}");
    }

    /// <inheritdoc />
    public async Task ClearSelectionAsync(string sessionId, CancellationToken ct = default)
    {
        EnsureSession(sessionId);
        await _selectionDao.DeleteBySessionAsync(sessionId);
        _audit.Log(sessionId, "ClearSelection", "all cleared");
    }

    /// <inheritdoc />
    public async Task<List<string>> GetSelectionAsync(string sessionId, CancellationToken ct = default)
    {
        EnsureSession(sessionId);
        return (List<string>)await _selectionDao.GetIdsBySessionAsync(sessionId);
    }

    private static void EnsureSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
    }
}

