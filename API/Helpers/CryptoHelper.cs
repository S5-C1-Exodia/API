using System.Security.Cryptography;
using System.Text;
using Api.Managers.InterfacesHelpers;

namespace API.Helpers;

public class CryptoHelper : ICryptoHelper
{
    public string GenerateState(int byteLength)
    {
        if (byteLength <= 0)
        {
            throw new ArgumentException("byteLength must be positive.", nameof(byteLength));
        }

        byte[] bytes = new byte[byteLength];
        RandomNumberGenerator.Fill(bytes);
        string result = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        return result;
    }

    public void GeneratePkce(out string codeVerifier, out string codeChallenge)
    {
        // RFC 7636: code_verifier = high-entropy cryptographic random string (43..128 chars)
        byte[] verifierBytes = new byte[32];
        RandomNumberGenerator.Fill(verifierBytes);
        string verifier = Convert.ToBase64String(verifierBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        byte[] sha256;
        using (SHA256 sha = SHA256.Create())
        {
            sha256 = sha.ComputeHash(Encoding.ASCII.GetBytes(verifier));
        }

        string challenge = Convert.ToBase64String(sha256)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        codeVerifier = verifier;
        codeChallenge = challenge;
    }
}