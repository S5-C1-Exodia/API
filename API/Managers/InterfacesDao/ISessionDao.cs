using Api.Models;

namespace Api.Managers.InterfacesDao;

public interface ISessionDao
{
    Task InsertAsync(AppSession session);
    Task<AppSession> GetAsync(string sessionId);
}