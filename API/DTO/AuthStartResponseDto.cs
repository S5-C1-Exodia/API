namespace API.DTO;

using System;

public class AuthStartResponseDto
{
    private string _authorizationUrl;
    private string _state;

    public AuthStartResponseDto(string authorizationUrl, string state)
    {
        this.AuthorizationUrl = authorizationUrl;
        this.State = state;
    }

    public string AuthorizationUrl
    {
        get { return this._authorizationUrl; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("AuthorizationUrl cannot be null or empty.", nameof(value));
            }
            this._authorizationUrl = value;
        }
    }

    public string State
    {
        get { return this._state; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("State cannot be null or empty.", nameof(value));
            }
            this._state = value;
        }
    }
}