namespace API.Managers.InterfacesServices;

using System.Threading.Tasks;

public interface ITokenDenylistService
{
    Task<bool> IsDeniedAsync(string refreshTokenHash);
    Task AddAsync(string refreshTokenHash, string reason, System.DateTime? expiresAtUtc);
}