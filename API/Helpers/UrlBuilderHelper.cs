using System.Web;
using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;

namespace API.Helpers;

public class UrlBuilderHelper(IConfigService config) : IUrlBuilderHelper
{
    private readonly IConfigService _config = config ?? throw new ArgumentNullException(nameof(config));

    public string BuildAuthorizeUrl(string clientId, string redirectUri, string[] scopes, string state, string codeChallenge,
        string codeChallengeMethod)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("clientId cannot be null or empty.", nameof(clientId));
        }

        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            throw new ArgumentException("redirectUri cannot be null or empty.", nameof(redirectUri));
        }

        if (scopes == null || scopes.Length == 0)
        {
            throw new ArgumentException("scopes cannot be null or empty.", nameof(scopes));
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("state cannot be null or empty.", nameof(state));
        }

        if (string.IsNullOrWhiteSpace(codeChallenge))
        {
            throw new ArgumentException("codeChallenge cannot be null or empty.", nameof(codeChallenge));
        }

        if (string.IsNullOrWhiteSpace(codeChallengeMethod))
        {
            throw new ArgumentException("codeChallengeMethod cannot be null or empty.", nameof(codeChallengeMethod));
        }

        string endpoint = _config.GetSpotifyAuthorizeEndpoint();
        string scopeJoined = string.Join(" ", scopes);

        UriBuilder builder = new UriBuilder(endpoint);
        string query = string.Empty;

        var qp = HttpUtility.ParseQueryString(query);
        qp["client_id"] = clientId;
        qp["response_type"] = "code";
        qp["redirect_uri"] = redirectUri;
        qp["scope"] = scopeJoined;
        qp["state"] = state;
        qp["code_challenge"] = codeChallenge;
        qp["code_challenge_method"] = codeChallengeMethod;

        builder.Query = qp.ToString();
        string url = builder.ToString();
        return url;
    }
}