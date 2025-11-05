using API.Managers.InterfacesServices;

namespace API.Services;

/// <summary>
/// Provides the current UTC date and time.
/// </summary>
public class ClockService : IClockService
{
    public DateTime GetUtcNow()
    {
        return DateTime.UtcNow;
    }
}