using Api.Models;

namespace Api.Managers.InterfacesDao;

public interface IPkceDao
{
    Task SaveAsync(PkceEntry entry);
    Task<PkceEntry> GetAsync(string state);
    Task DeleteAsync(string state);
}