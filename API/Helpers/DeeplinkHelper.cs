using Api.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;

namespace API.Helpers;

/// <summary>
/// Helper class for building deeplink URLs based on session identifiers and configuration.
/// </summary>
/// <param name="config">The configuration service used to retrieve the deeplink scheme and host.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
public class DeeplinkHelper(IConfigService config) : IDeeplinkHelper
{
    private readonly IConfigService _config = config ?? throw new ArgumentNullException(nameof(config));

    /// <summary>
    /// Builds a deeplink URL using the provided session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier to include in the deeplink. Must not be null or empty.</param>
    /// <returns>
    /// A deeplink URL string containing the session identifier as a query parameter.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is null or empty.</exception>
    public string BuildDeepLink(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("sessionId cannot be null or empty.", nameof(sessionId));
        }

        string schemeHost = _config.GetDeeplinkSchemeHost();
        string link = schemeHost + "?sid=" + Uri.EscapeDataString(sessionId);
        return link;
    }
}