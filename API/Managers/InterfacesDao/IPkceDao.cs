namespace Api.Managers.InterfacesDao;

using System.Threading.Tasks;
using Api.Models;

public interface IPkceDao
{
    Task SaveAsync(PkceEntry entry);
    Task<PkceEntry> GetAsync(string state);
    Task DeleteAsync(string state);
}