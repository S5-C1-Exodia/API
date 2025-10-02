namespace Api.Models
{
    using System;

    public class AppSession
    {
        private string _sessionId;
        private string _deviceInfo;
        private DateTime _createdAt;
        private DateTime _lastSeenAt;
        private DateTime _expiresAt;

        public AppSession(string sessionId, string deviceInfo, DateTime createdAt, DateTime lastSeenAt, DateTime expiresAt)
        {
            this.SessionId = sessionId;
            this.DeviceInfo = deviceInfo;
            this.CreatedAt = createdAt;
            this.LastSeenAt = lastSeenAt;
            this.ExpiresAt = expiresAt;
        }

        public string SessionId
        {
            get { return this._sessionId; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("SessionId cannot be null or empty.", nameof(value));
                }

                this._sessionId = value;
            }
        }

        public string DeviceInfo
        {
            get { return this._deviceInfo; }
            set { this._deviceInfo = value ?? string.Empty; }
        }

        public DateTime CreatedAt
        {
            get { return this._createdAt; }
            set { this._createdAt = value; }
        }

        public DateTime LastSeenAt
        {
            get { return this._lastSeenAt; }
            set { this._lastSeenAt = value; }
        }

        public DateTime ExpiresAt
        {
            get { return this._expiresAt; }
            set { this._expiresAt = value; }
        }
    }
}