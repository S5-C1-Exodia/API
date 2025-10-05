using System.Security.Cryptography;
using System.Text;
using API.Managers.InterfacesServices;

namespace API.Services
{
    public class HashService : IHashService
    {
        public string Sha256Base64(string? input)
        {
            input ??= string.Empty;
            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}