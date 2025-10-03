using API.Managers.InterfacesServices;

namespace API.Services;

public class IdGenerator : IIdGenerator
{
    public IdGenerator()
    {
    }

    public string NewSessionId()
    {
        // 32 chars hex sans tirets
        return Guid.NewGuid().ToString("N");
    }
}