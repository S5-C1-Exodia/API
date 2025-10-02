namespace API.DTO;

using System;

/// <summary>
/// DTO representing the response for starting authentication, including the authorization URL and state.
/// </summary>
public class AuthStartResponseDto
{
    private string _authorizationUrl;
    private string _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthStartResponseDto"/> class.
    /// </summary>
    /// <param name="authorizationUrl">The authorization URL to redirect the user to.</param>
    /// <param name="state">The state parameter for PKCE validation.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="authorizationUrl"/> or <paramref name="state"/> is null or empty.</exception>
    public AuthStartResponseDto(string authorizationUrl, string state)
    {
        this.AuthorizationUrl = authorizationUrl;
        this.State = state;
    }

    /// <summary>
    /// Gets or sets the authorization URL to redirect the user to.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if value is null or empty.</exception>
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

    /// <summary>
    /// Gets or sets the state parameter for PKCE validation.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if value is null or empty.</exception>
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