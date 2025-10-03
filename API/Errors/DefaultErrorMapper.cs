namespace API.Errors;

using System;

public class DefaultErrorMapper : IErrorMapper
{
    public ApiError Map(Exception exception, string correlationId, bool includeDetails, DateTime nowUtc, out int httpStatus)
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

        // Mapping précis de tes exceptions métier
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