using API.DTO;

namespace API.Controllers.InterfacesManagers;

public interface IAuthManager
{
    Task<AuthStartResponseDto> StartAuthAsync(IList<string> scopes);
    Task<string> HandleCallbackAsync(string code, string state, string deviceInfo);
}