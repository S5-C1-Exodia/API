using API.Managers.InterfacesServices;

namespace API.Services;

/// <inheritdoc />
public class IdGenerator : IIdGenerator
{
    /// <inheritdoc />
    public string NewSessionId()
    {
        return Guid.NewGuid().ToString("N");
    }
}