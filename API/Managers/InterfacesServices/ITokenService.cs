namespace API.Services;

public interface ITokenService
{
    /// <summary>
    /// Get access token
    /// </summary>
    /// <param name="sessionId">session id to check access token</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>the access token</returns>
    Task<string> GetAccessTokenAsync(string sessionId, CancellationToken ct = default);
}