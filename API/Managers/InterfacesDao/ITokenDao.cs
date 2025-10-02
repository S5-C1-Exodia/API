namespace Api.Managers.InterfacesDao;

public interface ITokenDao
{
    Task<long> SaveByStateAsync(string state, string provider, string providerUserId,
        string refreshToken, string scope, System.DateTime accessExpiresAt);

    Task AttachToSessionAsync(long tokenSetId, string sessionId);
}