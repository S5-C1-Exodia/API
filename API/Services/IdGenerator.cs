using API.Managers.InterfacesServices;

namespace API.Services;

/// <summary>
/// Service for generating unique session identifiers.
/// </summary>
public class IdGenerator : IIdGenerator
{
    /// <inheritdoc />
    public string NewSessionId()
    {
        return Guid.NewGuid().ToString("N");
    }
}