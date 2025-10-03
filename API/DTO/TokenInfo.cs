namespace API.DTO;

using System;

/// <summary>
/// DTO containing information about OAuth tokens and related metadata.
/// </summary>
public class TokenInfo
{
    private string _accessToken;
    private string _refreshToken;
    private DateTime _accessExpiresAt;
    private string _scope;
    private string _providerUserId;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenInfo"/> class.
    /// </summary>
    /// <param name="accessToken">The access token string.</param>
    /// <param name="refreshToken">The refresh token string.</param>
    /// <param name="accessExpiresAt">The UTC expiration date and time of the access token.</param>
    /// <param name="scope">The OAuth scopes granted.</param>
    /// <param name="providerUserId">The user ID at the OAuth provider.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="accessToken"/> or <paramref name="refreshToken"/> is null or empty.</exception>
    public TokenInfo(string accessToken, string refreshToken, DateTime accessExpiresAt, string scope, string providerUserId)
    {
        this.AccessToken = accessToken;
        this.RefreshToken = refreshToken;
        this.AccessExpiresAt = accessExpiresAt;
        this.Scope = scope;
        this.ProviderUserId = providerUserId;
    }

    /// <summary>
    /// Gets or sets the access token string.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if value is null or empty.</exception>
    public string AccessToken
    {
        get { return this._accessToken; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("AccessToken cannot be null or empty.", nameof(value));
            }

            this._accessToken = value;
        }
    }

    /// <summary>
    /// Gets or sets the refresh token string.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if value is null or empty.</exception>
    public string RefreshToken
    {
        get { return this._refreshToken; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("RefreshToken cannot be null or empty.", nameof(value));
            }

            this._refreshToken = value;
        }
    }

    /// <summary>
    /// Gets or sets the UTC expiration date and time of the access token.
    /// </summary>
    public DateTime AccessExpiresAt
    {
        get { return this._accessExpiresAt; }
        set { this._accessExpiresAt = value; }
    }

    /// <summary>
    /// Gets or sets the OAuth scopes granted.
    /// </summary>
    public string Scope
    {
        get { return this._scope; }
        set { this._scope = value ?? string.Empty; }
    }

    /// <summary>
    /// Gets or sets the user ID at the OAuth provider.
    /// </summary>
    public string ProviderUserId
    {
        get { return this._providerUserId; }
        set { this._providerUserId = value ?? string.Empty; }
    }
}