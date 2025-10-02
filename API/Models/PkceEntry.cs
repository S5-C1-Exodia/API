namespace Api.Models
{
    using System;

    public class PkceEntry
    {
        private string _state;
        private string _codeVerifier;
        private string _codeChallenge;
        private DateTime _expiresAt;

        public PkceEntry(string state, string codeVerifier, string codeChallenge, DateTime expiresAt)
        {
            this.State = state;
            this.CodeVerifier = codeVerifier;
            this.CodeChallenge = codeChallenge;
            this.ExpiresAt = expiresAt;
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

        public string CodeVerifier
        {
            get { return this._codeVerifier; }
            set { this._codeVerifier = value ?? string.Empty; }
        }

        public string CodeChallenge
        {
            get { return this._codeChallenge; }
            set { this._codeChallenge = value ?? string.Empty; }
        }

        public DateTime ExpiresAt
        {
            get { return this._expiresAt; }
            set { this._expiresAt = value; }
        }

        public bool IsExpired(DateTime nowUtc)
        {
            return nowUtc >= this._expiresAt;
        }
    }
}