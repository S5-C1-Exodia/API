using API.Controllers.InterfacesManagers;
using API.DTO;
using API.Errors;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using Api.Managers.InterfacesServices;
using API.Managers.InterfacesServices;
using Api.Models;

namespace API.Managers;

public class AuthManager : IAuthManager
{
    private readonly ICryptoHelper _crypto;
    private readonly IUrlBuilderHelper _urlBuilder;
    private readonly IPkceDao _pkceDao;
    private readonly ISpotifyOAuthHelper _oauth;
    private readonly ITokenDao _tokenDao;
    private readonly ISessionService _session;
    private readonly IDeeplinkHelper _deeplink;
    private readonly IClockService _clock;
    private readonly IConfigService _config;

    // US 1.3
    private readonly IAccessTokenDao _accessTokenDao;
    private readonly IPlaylistSelectionDao _playlistSelectionDao;
    private readonly IPlaylistCacheDao _playlistCacheDao;
    private readonly IUserProfileCacheDao _userProfileCacheDao;
    private readonly ITokenDenyListService _denylist;
    private readonly IHashService _hash;
    private readonly ISqlConnectionFactory _sqlFactory;
    private readonly IAuditService _audit;

    public AuthManager(
        ICryptoHelper crypto,
        IUrlBuilderHelper urlBuilder,
        IPkceDao pkceDao,
        ISpotifyOAuthHelper oauth,
        ITokenDao tokenDao,
        ISessionService session,
        IDeeplinkHelper deeplink,
        IClockService clock,
        IConfigService config,
        // US 1.3 deps
        IAccessTokenDao accessTokenDao,
        IPlaylistSelectionDao playlistSelectionDao,
        IPlaylistCacheDao playlistCacheDao,
        IUserProfileCacheDao userProfileCacheDao,
        ITokenDenyListService denylist,
        IHashService hash,
        ISqlConnectionFactory sqlFactory,
        IAuditService audit
    )
    {
        _crypto = crypto ?? throw new ArgumentNullException(nameof(crypto));
        _urlBuilder = urlBuilder ?? throw new ArgumentNullException(nameof(urlBuilder));
        _pkceDao = pkceDao ?? throw new ArgumentNullException(nameof(pkceDao));
        _oauth = oauth ?? throw new ArgumentNullException(nameof(oauth));
        _tokenDao = tokenDao ?? throw new ArgumentNullException(nameof(tokenDao));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _deeplink = deeplink ?? throw new ArgumentNullException(nameof(deeplink));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _accessTokenDao = accessTokenDao ?? throw new ArgumentNullException(nameof(accessTokenDao));
        _playlistSelectionDao = playlistSelectionDao ?? throw new ArgumentNullException(nameof(playlistSelectionDao));
        _playlistCacheDao = playlistCacheDao ?? throw new ArgumentNullException(nameof(playlistCacheDao));
        _userProfileCacheDao = userProfileCacheDao ?? throw new ArgumentNullException(nameof(userProfileCacheDao));
        _denylist = denylist ?? throw new ArgumentNullException(nameof(denylist));
        _hash = hash ?? throw new ArgumentNullException(nameof(hash));
        _sqlFactory = sqlFactory ?? throw new ArgumentNullException(nameof(sqlFactory));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
    }

    public async Task<AuthStartResponseDto> StartAuthAsync(IList<string> scopes)
    {
        if (scopes == null || scopes.Count == 0)
            throw new ArgumentException("scopes cannot be null or empty.", nameof(scopes));

        string state = _crypto.GenerateState(32);
        _crypto.GeneratePkce(out string codeVerifier, out string codeChallenge);

        DateTime now = _clock.GetUtcNow();
        DateTime exp = now.AddMinutes(_config.GetPkceTtlMinutes());

        var entry = new PkceEntry(state, codeVerifier, codeChallenge, exp);
        await _pkceDao.SaveAsync(entry);

        string url = _urlBuilder.BuildAuthorizeUrl(
            _config.GetSpotifyClientId(),
            _config.GetSpotifyRedirectUri(),
            scopes.ToArray(),
            state,
            codeChallenge,
            "S256"
        );

        return new AuthStartResponseDto(url, state);
    }

    public async Task<string> HandleCallbackAsync(string code, string state, string? deviceInfo)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("code cannot be null or empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("state cannot be null or empty.", nameof(state));

        var entry = await _pkceDao.GetAsync(state) ?? throw new InvalidStateException("Unknown state.");
        DateTime now = _clock.GetUtcNow();
        if (entry.IsExpired(now))
        {
            await _pkceDao.DeleteAsync(state);
            throw new InvalidStateException("Expired state.");
        }

        TokenInfo tokens;
        try
        {
            tokens = await _oauth.ExchangeCodeForTokensAsync(
                code,
                _config.GetSpotifyRedirectUri(),
                entry.CodeVerifier
            );
        }
        catch (Exception ex)
        {
            throw new TokenExchangeFailedException("Failed to exchange code for tokens.", ex);
        }

        long tokenSetId = await _tokenDao.SaveByStateAsync(
            state,
            "spotify",
            tokens.ProviderUserId,
            tokens.RefreshToken,
            tokens.Scope,
            tokens.AccessExpiresAt
        );

        string sessionId = await _session.CreateSessionAsync(
            deviceInfo ?? string.Empty,
            now,
            now.AddMinutes(_config.GetSessionTtlMinutes())
        );
        await _tokenDao.AttachToSessionAsync(tokenSetId, sessionId);
        await _pkceDao.DeleteAsync(state);

        return _deeplink.BuildDeepLink(sessionId);
    }

    /// <summary>
    ///     US 1.3 – Déconnexion sécurisée (denylist hors TX + purges atomiques + audit).
    /// </summary>
    public async Task LogoutAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));

        DateTime now = _clock.GetUtcNow();

        // 1) TokenSet lié à la session
        TokenSet? tokenSet = await _tokenDao.GetBySessionAsync(sessionId);

        // 2) Denylist du refresh (hors transaction)
        if (!string.IsNullOrWhiteSpace(tokenSet?.RefreshTokenEnc))
        {
            string refresh = tokenSet.RefreshTokenEnc; // en clair pour l’instant
            string hash = _hash.Sha256Base64(refresh);
            await _denylist.AddAsync(hash, "logout", now.AddDays(90));
        }

        // 3) Purges en transaction unique
        await using var conn = _sqlFactory.Create();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            await _accessTokenDao.DeleteBySessionAsync(sessionId, conn, tx);
            await _playlistSelectionDao.DeleteBySessionAsync(sessionId, conn, tx);

            if (!string.IsNullOrWhiteSpace(tokenSet?.ProviderUserId))
            {
                await _playlistCacheDao.DeleteByProviderUserAsync(tokenSet.ProviderUserId, conn, tx);
                await _userProfileCacheDao.DeleteByProviderUserAsync(tokenSet.ProviderUserId, conn, tx);
            }

            await _playlistCacheDao.DeleteLinksBySessionAsync(sessionId, conn, tx);
            await _tokenDao.DeleteBySessionAsync(sessionId, conn, tx);
            await _session.DeleteAsync(sessionId, conn, tx);

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        // 4) Audit
        _audit.LogAuth("spotify", "SpotifyLogout", "purge DB + denylist");
    }
}