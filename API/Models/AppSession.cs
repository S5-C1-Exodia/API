namespace Api.Models
{
    using System;

    /// <summary>
    /// Represents an application session for a user/device.
    /// </summary>
    public class AppSession
    {
        private string _sessionId;
        private string _deviceInfo;
        private DateTime _createdAt;
        private DateTime _lastSeenAt;
        private DateTime _expiresAt;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSession"/> class.
        /// </summary>
        /// <param name="sessionId">The unique session identifier.</param>
        /// <param name="deviceInfo">Information about the device.</param>
        /// <param name="createdAt">The UTC creation date and time.</param>
        /// <param name="lastSeenAt">The UTC date and time the session was last seen.</param>
        /// <param name="expiresAt">The UTC expiration date and time.</param>
        public AppSession(string sessionId, string deviceInfo, DateTime createdAt, DateTime lastSeenAt, DateTime expiresAt)
        {
            this.SessionId = sessionId;
            this.DeviceInfo = deviceInfo;
            this.CreatedAt = createdAt;
            this.LastSeenAt = lastSeenAt;
            this.ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Gets or sets the unique session identifier.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if value is null or empty.</exception>
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

        /// <summary>
        /// Gets or sets the device information.
        /// </summary>
        public string DeviceInfo
        {
            get { return this._deviceInfo; }
            set { this._deviceInfo = value ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets the UTC creation date and time.
        /// </summary>
        public DateTime CreatedAt
        {
            get { return this._createdAt; }
            set { this._createdAt = value; }
        }

        /// <summary>
        /// Gets or sets the UTC date and time the session was last seen.
        /// </summary>
        public DateTime LastSeenAt
        {
            get { return this._lastSeenAt; }
            set { this._lastSeenAt = value; }
        }

        /// <summary>
        /// Gets or sets the UTC expiration date and time.
        /// </summary>
        public DateTime ExpiresAt
        {
            get { return this._expiresAt; }
            set { this._expiresAt = value; }
        }
    }
}