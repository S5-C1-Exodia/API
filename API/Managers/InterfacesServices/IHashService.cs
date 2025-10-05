namespace API.Managers.InterfacesServices;

public interface IHashService
{
    string Sha256Base64(string input);
}