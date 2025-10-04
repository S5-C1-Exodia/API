namespace Api.Managers.InterfacesDao;

public interface IAccessTokenDao
{
    Task DeleteBySessionAsync(string sessionId);
}