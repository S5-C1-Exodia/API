namespace Api.Managers.InterfacesHelpers;

public interface IUrlBuilderHelper
{
    string BuildAuthorizeUrl(string clientId, string redirectUri, string[] scopes, string state, string codeChallenge,
        string codeChallengeMethod);
}