using System.Text.RegularExpressions;
using API.Helpers;

namespace Tests.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="CryptoHelper"/>.
    /// </summary>
    public class CryptoHelperTests
    {
        /// <summary>
        /// Tests that <see cref="CryptoHelper.GenerateState(int)"/> returns a URL-safe Base64 string.
        /// </summary>
        [Fact]
        public void GenerateState_ShouldReturnUrlSafeBase64_AndNotEmpty()
        {
            CryptoHelper helper = new CryptoHelper();

            string s = helper.GenerateState(32);

            Assert.False(string.IsNullOrWhiteSpace(s));
            // Vérifie que c'est bien base64url (pas de + / =)
            Regex urlSafe = new Regex("^[A-Za-z0-9_-]+$");
            Assert.Matches(urlSafe, s);
            Assert.True(s.Length >= 10);
        }

        /// <summary>
        /// Tests that <see cref="CryptoHelper.GenerateState(int)"/> throws an <see cref="ArgumentException"/> for invalid lengths.
        /// </summary>
        [Fact]
        public void GenerateState_ShouldThrow_IfLengthInvalid()
        {
            CryptoHelper helper = new CryptoHelper();

            Assert.Throws<ArgumentException>(() => helper.GenerateState(0));
            Assert.Throws<ArgumentException>(() => helper.GenerateState(-1));
        }

        /// <summary>
        /// Tests that <see cref="CryptoHelper.GeneratePkce(out string, out string)"/> produces a valid verifier and challenge.
        /// </summary>
        [Fact]
        public void GeneratePkce_ShouldProduceVerifierAndChallenge_UrlSafe()
        {
            CryptoHelper helper = new CryptoHelper();

            string verifier;
            string challenge;
            helper.GeneratePkce(out verifier, out challenge);

            Assert.False(string.IsNullOrWhiteSpace(verifier));
            Assert.False(string.IsNullOrWhiteSpace(challenge));

            // URL-safe
            Regex urlSafe = new Regex("^[A-Za-z0-9_-]+$");
            Assert.Matches(urlSafe, verifier);
            Assert.Matches(urlSafe, challenge);

            // Longueur minimale RFC (43..128) — ici on génère ~43-48 chars (base64url de 32 bytes)
            Assert.True(verifier.Length >= 40);
            Assert.True(challenge.Length >= 40);
        }
    }
}