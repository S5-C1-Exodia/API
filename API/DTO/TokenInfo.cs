namespace API.DTO;

using System;

public class TokenInfo
{
    private string _accessToken;
    private string _refreshToken;
    private DateTime _accessExpiresAt;
    private string _scope;
    private string _providerUserId;

    public TokenInfo(string accessToken, string refreshToken, DateTime accessExpiresAt, string scope, string providerUserId)
    {
        this.AccessToken = accessToken;
        this.RefreshToken = refreshToken;
        this.AccessExpiresAt = accessExpiresAt;
        this.Scope = scope;
        this.ProviderUserId = providerUserId;
    }

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

    public DateTime AccessExpiresAt
    {
        get { return this._accessExpiresAt; }
        set { this._accessExpiresAt = value; }
    }

    public string Scope
    {
        get { return this._scope; }
        set { this._scope = value ?? string.Empty; }
    }

    public string ProviderUserId
    {
        get { return this._providerUserId; }
        set { this._providerUserId = value ?? string.Empty; }
    }
}