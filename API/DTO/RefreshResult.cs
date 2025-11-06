namespace API.DTO;

/// <summary>
/// Represents the result of an OAuth refresh-token call.
/// Contains the new access token, its expiration timestamp (UTC),
/// and an optional rotated refresh token if the provider returned one.
/// </summary>
public class RefreshResult
{
    /// <summary>
    /// Gets the new access token returned by the OAuth provider.
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// Gets the UTC timestamp when the access token expires.
    /// </summary>
    public DateTime AccessExpiresAtUtc { get; }

    /// <summary>
    /// Gets the new refresh token if the provider returned one; otherwise, null.
    /// </summary>
    public string? NewRefreshToken { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshResult"/> class.
    /// </summary>
    /// <param name="accessToken">The new access token returned by the OAuth provider. Must not be null or empty.</param>
    /// <param name="accessExpiresAtUtc">The UTC timestamp when the access token expires.</param>
    /// <param name="newRefreshToken">The new refresh token if the provider returned one; otherwise, null.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="accessToken"/> is null or empty.</exception>
    public RefreshResult(string accessToken, DateTime accessExpiresAtUtc, string? newRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("accessToken cannot be null or empty.", nameof(accessToken));

        AccessToken = accessToken;
        AccessExpiresAtUtc = accessExpiresAtUtc;
        NewRefreshToken = newRefreshToken;
    }
}