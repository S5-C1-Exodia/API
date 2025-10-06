namespace API.Errors.Exceptions
{
    /// <summary>
    /// Thrown when no TokenSet is associated with the provided session.
    /// </summary>
    public class MissingTokenSetException(string message) : Exception(message);
}