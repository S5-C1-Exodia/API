namespace Api.Models
{
    using System;

    public class TokenSet
    {
        private long _tokenSetId;
        private string _provider;
        private string _providerUserId;
        private string _refreshTokenEnc;
        private string _scope;
        private DateTime _accessExpiresAt;
        private DateTime _updatedAt;
        private string _sessionId;

        public TokenSet(long tokenSetId, string provider, string providerUserId, string refreshTokenEnc,
            string scope, DateTime accessExpiresAt, DateTime updatedAt, string sessionId)
        {
            this.TokenSetId = tokenSetId;
            this.Provider = provider;
            this.ProviderUserId = providerUserId;
            this.RefreshTokenEnc = refreshTokenEnc;
            this.Scope = scope;
            this.AccessExpiresAt = accessExpiresAt;
            this.UpdatedAt = updatedAt;
            this.SessionId = sessionId;
        }

        public long TokenSetId
        {
            get { return this._tokenSetId; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("TokenSetId must be positive.", nameof(value));
                }

                this._tokenSetId = value;
            }
        }

        public string Provider
        {
            get { return this._provider; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Provider cannot be null or empty.", nameof(value));
                }

                this._provider = value;
            }
        }

        public string ProviderUserId
        {
            get { return this._providerUserId; }
            set { this._providerUserId = value ?? string.Empty; }
        }

        public string RefreshTokenEnc
        {
            get { return this._refreshTokenEnc; }
            set { this._refreshTokenEnc = value ?? string.Empty; }
        }

        public string Scope
        {
            get { return this._scope; }
            set { this._scope = value ?? string.Empty; }
        }

        public DateTime AccessExpiresAt
        {
            get { return this._accessExpiresAt; }
            set { this._accessExpiresAt = value; }
        }

        public DateTime UpdatedAt
        {
            get { return this._updatedAt; }
            set { this._updatedAt = value; }
        }

        public string SessionId
        {
            get { return this._sessionId; }
            set { this._sessionId = value ?? string.Empty; }
        }
    }
}