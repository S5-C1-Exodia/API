namespace API.Errors;

/// <summary>
/// Exception thrown when the token exchange with the OAuth provider fails.
/// </summary>
public class TokenExchangeFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenExchangeFailedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TokenExchangeFailedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenExchangeFailedException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public TokenExchangeFailedException(string message, Exception inner) : base(message, inner)
    {
    }
}