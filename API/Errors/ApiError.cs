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
        get => this._code;
        set => this._code = string.IsNullOrWhiteSpace(value) ? "error" : value;
    }

    public string Message
    {
        get => this._message;
        set => this._message = value ?? string.Empty;
    }

    public string CorrelationId
    {
        get => this._correlationId;
        set => this._correlationId = value ?? string.Empty;
    }

    public DateTime Timestamp
    {
        get => this._timestamp;
        set => this._timestamp = value;
    }

    public string Details
    {
        get => this._details;
        set => this._details = value ?? string.Empty;
    }
}