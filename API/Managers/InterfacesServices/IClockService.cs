namespace API.Managers.InterfacesServices;

    /// <summary>
    /// Provides the current UTC date and time.
    /// </summary>
public interface IClockService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <returns>The current UTC <see cref="DateTime"/>.</returns>
    DateTime GetUtcNow();
}