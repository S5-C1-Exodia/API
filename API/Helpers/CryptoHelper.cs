using System.Security.Cryptography;
        using System.Text;
        using Api.Managers.InterfacesHelpers;
        
        namespace API.Helpers;
        
        /// <summary>
        /// Provides cryptographic helper methods for generating secure random states and PKCE (Proof Key for Code Exchange) values.
        /// </summary>
        public class CryptoHelper : ICryptoHelper
        {
            /// <summary>
            /// Generates a secure random state string, URL-safe and base64-encoded, of the specified byte length.
            /// </summary>
            /// <param name="byteLength">The number of random bytes to generate. Must be positive.</param>
            /// <returns>
            /// A URL-safe base64-encoded string representing the random state.
            /// </returns>
            /// <exception cref="ArgumentException">Thrown if <paramref name="byteLength"/> is not positive.</exception>
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
        
            /// <summary>
            /// Generates a PKCE (Proof Key for Code Exchange) code verifier and its corresponding code challenge.
            /// </summary>
            /// <param name="codeVerifier">When this method returns, contains the generated code verifier (URL-safe base64 string).</param>
            /// <param name="codeChallenge">When this method returns, contains the generated code challenge (SHA256 hash of the verifier, URL-safe base64 string).</param>
            /// <remarks>
            /// The code verifier is a high-entropy cryptographic random string, and the code challenge is its SHA256 hash, both encoded in a URL-safe base64 format.
            /// </remarks>
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