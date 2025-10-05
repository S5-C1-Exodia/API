using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.DTO;
using API.Errors;
using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;

namespace API.Helpers;

public class SpotifyOAuthHelper(
    IHttpClientFactory httpClientFactory,
    IConfigService config,
    IClockService clock,
    IAuditService audit)
    : ISpotifyOAuthHelper
{
    private readonly IHttpClientFactory _httpClientFactory =
        httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    private readonly IConfigService _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly IClockService _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    private readonly IAuditService
        _audit = audit ?? throw new ArgumentNullException(nameof(audit)); // optionnel (hook Sprint 1+)

    public async Task<TokenInfo> ExchangeCodeForTokensAsync(string code, string redirectUri, string codeVerifier)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("code cannot be null or empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(redirectUri))
            throw new ArgumentException("redirectUri cannot be null or empty.", nameof(redirectUri));
        if (string.IsNullOrWhiteSpace(codeVerifier))
            throw new ArgumentException("codeVerifier cannot be null or empty.", nameof(codeVerifier));


        string tokenEndpoint = _config.GetSpotifyTokenEndpoint();
        string clientId = _config.GetSpotifyClientId();

        HttpClient http = _httpClientFactory.CreateClient("spotify-oauth");
        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);

        string body = "grant_type=authorization_code"
                      + "&code=" + Uri.EscapeDataString(code)
                      + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                      + "&client_id=" + Uri.EscapeDataString(clientId)
                      + "&code_verifier=" + Uri.EscapeDataString(codeVerifier);

        req.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

        HttpResponseMessage resp;
        try
        {
            resp = await http.SendAsync(req);
        }
        catch (Exception ex)
        {
            _audit.LogAuth("spotify", "TokenExchange.NetworkError", ex.Message);
            throw new TokenExchangeFailedException("Network error during token exchange.", ex);
        }

        string payload = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            string detail = "HTTP " + ((int)resp.StatusCode).ToString() + " payload: " + payload;
            _audit.LogAuth("spotify", "TokenExchange.HttpError", detail);

            if ((int)resp.StatusCode == 400)
                throw new TokenExchangeFailedException("Invalid authorization code or PKCE verifier.");

            if ((int)resp.StatusCode == 429)
                throw new TokenExchangeFailedException("Rate limited by Spotify during token exchange.");

            throw new TokenExchangeFailedException("Spotify token endpoint returned an error.");
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(payload);
        }
        catch (Exception ex)
        {
            _audit.LogAuth("spotify", "TokenExchange.ParseError", ex.Message);
            throw new TokenExchangeFailedException("Failed to parse token response.", ex);
        }

        string accessToken = ReadString(doc, "access_token");
        string refreshToken = ReadString(
            doc,
            "refresh_token"
        ); 
        string scope = ReadString(doc, "scope");
        int expiresIn = ReadInt(doc, "expires_in", 3600);

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new TokenExchangeFailedException("Token response missing access_token.");

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new TokenExchangeFailedException("Token response missing refresh_token.");

        DateTime now = _clock.GetUtcNow();
        DateTime accessExpiresAt = now.AddSeconds(expiresIn > 60 ? expiresIn - 60 : expiresIn);

        string meEndpoint = "https://api.spotify.com/v1/me";
        HttpRequestMessage meReq = new HttpRequestMessage(HttpMethod.Get, meEndpoint);
        meReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage meResp;
        try
        {
            meResp = await http.SendAsync(meReq);
        }
        catch (Exception ex)
        {
            _audit.LogAuth("spotify", "Me.NetworkError", ex.Message);
            throw new TokenExchangeFailedException("Network error fetching Spotify profile.", ex);
        }

        string mePayload = await meResp.Content.ReadAsStringAsync();

        if (!meResp.IsSuccessStatusCode)
        {
            string detail = "HTTP " + ((int)meResp.StatusCode).ToString() + " payload: " + mePayload;
            _audit.LogAuth("spotify", "Me.HttpError", detail);
            throw new TokenExchangeFailedException("Failed to fetch Spotify user profile.");
        }

        JsonDocument meDoc;
        try
        {
            meDoc = JsonDocument.Parse(mePayload);
        }
        catch (Exception ex)
        {
            _audit.LogAuth("spotify", "Me.ParseError", ex.Message);
            throw new TokenExchangeFailedException("Failed to parse Spotify user profile.", ex);
        }

        string providerUserId = ReadString(meDoc, "id");
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new TokenExchangeFailedException("Spotify profile missing id.");

        TokenInfo tokenInfo = new TokenInfo(accessToken, refreshToken, accessExpiresAt, scope, providerUserId);
        _audit.LogAuth("spotify", "AuthSuccess", "UserId=" + providerUserId);
        return tokenInfo;
    }

    private string ReadString(JsonDocument doc, string property)
    {
        if (doc == null) return string.Empty;
        if (!doc.RootElement.TryGetProperty(property, out JsonElement el)) return string.Empty;
        if (el.ValueKind == JsonValueKind.String) return el.GetString() ?? string.Empty;
        return el.ToString();
    }

    private int ReadInt(JsonDocument doc, string property, int defaultValue)
    {
        if (doc == null) return defaultValue;
        if (!doc.RootElement.TryGetProperty(property, out JsonElement el)) return defaultValue;

        if (el.ValueKind == JsonValueKind.Number)
        {
            int value;
            bool ok = el.TryGetInt32(out value);
            if (ok) return value;
        }

        return defaultValue;
    }
}