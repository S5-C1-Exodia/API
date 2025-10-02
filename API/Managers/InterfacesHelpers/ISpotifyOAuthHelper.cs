using API.DTO;

namespace Api.Managers.InterfacesHelpers;

public interface ISpotifyOAuthHelper
{
    Task<TokenInfo> ExchangeCodeForTokensAsync(string code, string redirectUri, string codeVerifier);
}