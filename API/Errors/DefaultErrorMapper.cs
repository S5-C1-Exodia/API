namespace API.Errors;

using System;

/// <summary>
/// Default implementation of <see cref="IErrorMapper"/> for mapping exceptions to standardized <see cref="ApiError"/> responses.
/// Handles various known exception types and assigns appropriate error codes, messages, and HTTP status codes.
/// </summary>
public class DefaultErrorMapper : IErrorMapper
{
    /// <summary>
    /// Maps an <see cref="Exception"/> to an <see cref="ApiError"/> and determines the corresponding HTTP status code.
    /// </summary>
    /// <param name="exception">The exception to map. If null, a generic unknown error is returned.</param>
    /// <param name="correlationId">A correlation identifier for tracing the error (can be null).</param>
    /// <param name="includeDetails">If true, includes detailed exception information in the error response.</param>
    /// <param name="nowUtc">The current UTC timestamp to use for the error.</param>
    /// <param name="httpStatus">When this method returns, contains the HTTP status code corresponding to the error.</param>
    /// <returns>
    /// An <see cref="ApiError"/> representing the mapped error.
    /// </returns>
    public ApiError Map(Exception? exception, string correlationId, bool includeDetails, DateTime nowUtc, out int httpStatus)
    {
        if (exception == null)
        {
            httpStatus = 500;
            return new ApiError("error.unknown", "Unknown error.", correlationId, nowUtc, string.Empty);
        }

        string code = "error.unhandled";
        string message = "An unexpected error occurred.";
        string details = includeDetails ? exception.ToString() : string.Empty;
        int status = 500;

        if (exception is InvalidStateException)
        {
            code = "error.invalid_state";
            message = exception.Message;
            status = 400;
        }
        else if (exception is TokenExchangeFailedException)
        {
            code = "error.token_exchange_failed";
            message = exception.Message;
            status = 502;
        }
        else if (exception is ArgumentException)
        {
            code = "error.bad_request";
            message = exception.Message;
            status = 400;
        }
        else if (exception is UnauthorizedAccessException)
        {
            code = "error.unauthorized";
            message = "Unauthorized.";
            status = 401;
        }
        else if (exception is NotImplementedException)
        {
            code = "error.not_implemented";
            message = "Not implemented.";
            status = 501;
        }

        httpStatus = status;
        ApiError error = new ApiError(code, message, correlationId, nowUtc, details);
        return error;
    }
}