namespace API.Errors;

public class InvalidStateException : Exception
{
    public InvalidStateException(string message) : base(message)
    {
    }

    public InvalidStateException(string message, Exception inner) : base(message, inner)
    {
    }
}