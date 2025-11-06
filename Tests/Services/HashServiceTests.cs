using API.Services;

namespace Tests.Services
{
    public class HashServiceTests
    {
        [Fact]
        public void Sha256Base64_ShouldReturnKnownHash_ForAbc()
        {
            var svc = new HashService();
            // SHA-256("abc") en Base64
            string result = svc.Sha256Base64("abc");
            Assert.Equal("ungWv48Bz+pBQUDeXa4iI7ADYaOWF3qctBD/YfIAFa0=", result);
        }

        [Fact]
        public void Sha256Base64_ShouldReturnKnownHash_ForEmptyString()
        {
            var svc = new HashService();
            // SHA-256("") en Base64
            string result = svc.Sha256Base64(string.Empty);
            Assert.Equal("47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=", result);
        }

        [Fact]
        public void Sha256Base64_NullIsTreatedAsEmpty()
        {
            var svc = new HashService();
            // L'implémentation actuelle convertit null -> "" avant hash
            string resultNull = svc.Sha256Base64(null!);
            string resultEmpty = svc.Sha256Base64("");
            Assert.Equal(resultEmpty, resultNull);
        }
    }
}