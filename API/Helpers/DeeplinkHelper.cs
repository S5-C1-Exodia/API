using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;

namespace API.Helpers;

public class DeeplinkHelper(IConfigService config) : IDeeplinkHelper
{
    private readonly IConfigService _config = config ?? throw new ArgumentNullException(nameof(config));

    public string BuildDeepLink(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        }

        // Exemple: swipez://oauth-callback/spotify?sid=...
        string schemeHost = this._config.GetDeeplinkSchemeHost(); // ex: "swipez://oauth-callback/spotify"
        string link = schemeHost + "?sid=" + Uri.EscapeDataString(sessionId);
        return link;
    }
}
