namespace API.Errors;

/// <summary>
/// Exception thrown when the PKCE state is invalid or expired.
/// </summary>
public class InvalidStateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStateException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidStateException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStateException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public InvalidStateException(string message, Exception inner) : base(message, inner)
    {
    }
}