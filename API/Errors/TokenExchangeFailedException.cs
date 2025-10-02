namespace API.Errors;

public class TokenExchangeFailedException : Exception
{
    public TokenExchangeFailedException(string message) : base(message)
    {
    }

    public TokenExchangeFailedException(string message, Exception inner) : base(message, inner)
    {
    }
}