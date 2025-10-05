namespace API.Errors;

using System;

public class ApiError(string code, string message, string correlationId, DateTime timestamp, string details)
{
    private string _code = code;
    private string _message = message;
    private string _correlationId = correlationId;
    private DateTime _timestamp = timestamp;
    private string _details = details;

    public string Code
    {
        get => _code;
        set => _code = string.IsNullOrWhiteSpace(value) ? "error" : value;
    }

    public string Message
    {
        get => _message;
        set => _message = value ?? string.Empty;
    }

    public string CorrelationId
    {
        get => _correlationId;
        set => _correlationId = value ?? string.Empty;
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public string Details
    {
        get => _details;
        set => _details = value ?? string.Empty;
    }
}