using API.Managers.InterfacesServices;

namespace API.Services;

public class ClockService : IClockService
{
    public DateTime GetUtcNow()
    {
        return DateTime.UtcNow;
    }
}