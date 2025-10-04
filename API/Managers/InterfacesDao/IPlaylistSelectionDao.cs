namespace Api.Managers.InterfacesDao;

public interface IPlaylistSelectionDao
{
    Task DeleteBySessionAsync(string sessionId);
}