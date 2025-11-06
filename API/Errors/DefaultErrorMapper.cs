namespace API.Errors;

/// <summary>
/// Default implementation of <see cref="IErrorMapper"/> for mapping exceptions to standardized <see cref="ApiError"/> responses.
/// Handles various known exception types and assigns appropriate error codes, messages, and HTTP status codes.
/// </summary>
public class DefaultErrorMapper : IErrorMapper
{
    private static readonly Dictionary<Type, (string Code, string DefaultMessage, int Status)> ExceptionMappings =
        new Dictionary<Type, (string Code, string DefaultMessage, int Status)>
        {
            { typeof(InvalidStateException), ("error.invalid_state", "Invalid state.", 400) },
            { typeof(TokenExchangeFailedException), ("error.token_exchange_failed", "Token exchange failed.", 502) },
            { typeof(ArgumentException), ("error.bad_request", "Bad request.", 400) },
            { typeof(UnauthorizedAccessException), ("error.unauthorized", "Unauthorized.", 401) },
            { typeof(NotImplementedException), ("error.not_implemented", "Not implemented.", 501) }
        };

    /// <inheritdoc />
    public ApiError Map(Exception? exception, string correlationId, bool includeDetails, DateTime nowUtc, out int httpStatus)
    {
        if (exception == null)
        {
            httpStatus = 500;
            return new ApiError("error.unknown", "Unknown error.", correlationId, nowUtc, string.Empty);
        }

        string details = includeDetails ? exception.ToString() : string.Empty;

        if (ExceptionMappings.TryGetValue(exception.GetType(), out var mapping))
        {
            httpStatus = mapping.Status;
            return new ApiError(mapping.Code, exception.Message, correlationId, nowUtc, details);
        }

        httpStatus = 500;
        return new ApiError("error.unhandled", "An unexpected error occurred.", correlationId, nowUtc, details);
    }
}