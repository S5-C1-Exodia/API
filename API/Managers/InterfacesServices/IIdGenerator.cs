namespace API.Managers.InterfacesServices;

    /// <summary>
    /// Provides methods to generate unique identifiers.
    /// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// Generates a new unique session identifier.
    /// </summary>
    /// <returns>A new session ID string.</returns>
    string NewSessionId();
}