using System.Security.Cryptography;
using System.Text;
using Api.Managers.InterfacesHelpers;

namespace API.Helpers
{
    /// <summary>
    /// Fournit des méthodes utilitaires cryptographiques.
    /// </summary>
    public class CryptoHelper : ICryptoHelper
    {
        private static string ToBase64Url(byte[] data)
        {
            return Convert.ToBase64String(data)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private static byte[] RandomBytes(int length)
        {
            if (length <= 0) throw new ArgumentException("length must be positive.", nameof(length));
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }

        /// <inheritdoc />
        public string GenerateState(int byteLength)
        {
            if (byteLength <= 0)
                throw new ArgumentException("byteLength must be positive.", nameof(byteLength));

            return ToBase64Url(RandomBytes(byteLength));
        }

        /// <inheritdoc />
        public void GeneratePkce(out string codeVerifier, out string codeChallenge)
        {
            // RFC 7636: code_verifier = high-entropy cryptographic random string (43..128 chars)
            byte[] verifierBytes = RandomBytes(32);
            codeVerifier = ToBase64Url(verifierBytes);

            byte[] sha256;
            using (var sha = SHA256.Create())
            {
                sha256 = sha.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            }

            codeChallenge = ToBase64Url(sha256);
        }
    }
}