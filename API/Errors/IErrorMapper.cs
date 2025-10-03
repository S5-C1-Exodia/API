namespace API.Errors;

using System;

public interface IErrorMapper
{
    ApiError Map(Exception exception, string correlationId, bool includeDetails, System.DateTime nowUtc, out int httpStatus);
}