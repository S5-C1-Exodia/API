namespace Api.Managers.InterfacesHelpers;

/// <summary>
/// Provides methods to build deeplink URLs for the mobile application.
/// </summary>
public interface IDeeplinkHelper
{
    /// <summary>
    /// Builds a deeplink URL for the given session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The deeplink URL as a string.</returns>
    string BuildDeepLink(string sessionId);
}