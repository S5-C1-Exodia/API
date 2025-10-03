using API.Controllers.InterfacesManagers;
using API.DTO;
using API.Errors;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using Api.Managers.InterfacesServices;
using API.Managers.InterfacesServices;
using Api.Models;

namespace API.Managers;

/// <summary>
/// Handles authentication logic, including PKCE flow, token exchange, and session management.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthManager"/> class.
    /// </summary>
    /// <param name="crypto">The cryptography helper.</param>
    /// <param name="urlBuilder">The URL builder helper.</param>
    /// <param name="pkceDao">The PKCE DAO.</param>
    /// <param name="oauth">The Spotify OAuth helper.</param>
    /// <param name="tokenDao">The token DAO.</param>
    /// <param name="session">The session service.</param>
    /// <param name="deeplink">The deeplink helper.</param>
    /// <param name="clock">The clock service.</param>
    /// <param name="config">The configuration service.</param>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
    public AuthManager(
        ICryptoHelper crypto,
        IUrlBuilderHelper urlBuilder,
        IPkceDao pkceDao,
        ISpotifyOAuthHelper oauth,
        ITokenDao tokenDao,
        ISessionService session,
        IDeeplinkHelper deeplink,
        IClockService clock,
        IConfigService config)
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
    }

    /// <summary>
    /// Starts the PKCE authentication process and returns the authorization URL and state.
    /// </summary>
    /// <param name="scopes">The list of scopes requested for authentication.</param>
    /// <returns>
    /// An <see cref="AuthStartResponseDto"/> containing the authorization URL and state.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
    public async Task<AuthStartResponseDto> StartAuthAsync(IList<string> scopes)
    {
        if (scopes == null || scopes.Count == 0)
        {
            throw new ArgumentException("scopes cannot be null or empty.", nameof(scopes));
        }

        string state = _crypto.GenerateState(32);
        string codeVerifier;
        string codeChallenge;
        _crypto.GeneratePkce(out codeVerifier, out codeChallenge);

        DateTime now = _clock.GetUtcNow();
        DateTime exp = now.AddMinutes(_config.GetPkceTtlMinutes());

        PkceEntry entry = new PkceEntry(state, codeVerifier, codeChallenge, exp);
        await _pkceDao.SaveAsync(entry);

        string[] scopeArray = scopes.ToArray();
        string url = _urlBuilder.BuildAuthorizeUrl(
            _config.GetSpotifyClientId(),
            _config.GetSpotifyRedirectUri(),
            scopeArray,
            state,
            codeChallenge,
            "S256"
        );

        AuthStartResponseDto response = new AuthStartResponseDto(url, state);
        return response;
    }

    /// <summary>
    /// Handles the OAuth callback, exchanges the code for tokens, creates a session, and returns the deeplink.
    /// </summary>
    /// <param name="code">The authorization code returned by the OAuth provider.</param>
    /// <param name="state">The PKCE state parameter.</param>
    /// <param name="deviceInfo">Optional device information.</param>
    /// <returns>
    /// The deeplink URL to redirect the user to the mobile app.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="code"/> or <paramref name="state"/> is null or empty.</exception>
    /// <exception cref="InvalidStateException">Thrown if the state is unknown or expired.</exception>
    /// <exception cref="TokenExchangeFailedException">Thrown if the token exchange fails.</exception>
    public async Task<string> HandleCallbackAsync(string code, string state, string? deviceInfo)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("code cannot be null or empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("state cannot be null or empty.", nameof(state));
        }

        PkceEntry entry = await _pkceDao.GetAsync(state);
        if (entry == null)
        {
            throw new InvalidStateException("Unknown state.");
        }

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

        DateTime sessionExp = now.AddMinutes(_config.GetSessionTtlMinutes());
        string safeDeviceInfo = deviceInfo ?? string.Empty;
        string sessionId = await _session.CreateSessionAsync(safeDeviceInfo, now, sessionExp);
        await _tokenDao.AttachToSessionAsync(tokenSetId, sessionId);
        await _pkceDao.DeleteAsync(state);

        string deepLink = _deeplink.BuildDeepLink(sessionId);
        return deepLink;
    }
}