using Api.Models;
using MySqlConnector;

namespace Api.Managers.InterfacesDao;

/// <summary>
/// Interface for token data access operations.
/// </summary>
public interface ITokenDao
{
    Task<long> SaveByStateAsync(string state, string provider, string providerUserId,
        string refreshToken, string scope, DateTime accessExpiresAt);

    Task AttachToSessionAsync(long tokenSetId, string sessionId);

    Task<TokenSet?> GetBySessionAsync(string sessionId);
    Task DeleteBySessionAsync(string sessionId);
    Task DeleteBySessionAsync(string sessionId, MySqlConnection conn, MySqlTransaction tx);
}