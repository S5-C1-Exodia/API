using API.Managers.InterfacesServices;

namespace API.Services;

/// <summary>
/// Service for generating unique identifiers for sessions.
/// </summary>
public class IdGenerator : IIdGenerator
{
    /// <summary>
    /// Generates a new unique session ID.
    /// </summary>
    /// <returns>A 32-character hexadecimal string without dashes.</returns>
    public string NewSessionId()
    {
        // 32 chars hex sans tirets
        return Guid.NewGuid().ToString("N");
    }
}