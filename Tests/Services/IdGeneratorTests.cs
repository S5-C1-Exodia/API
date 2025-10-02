using System.Text.RegularExpressions;
using API.Services;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="IdGenerator"/>.
    /// </summary>
    public class IdGeneratorTests
    {
        /// <summary>
        /// Tests that <see cref="IdGenerator.NewSessionId"/> generates a 32-character hexadecimal string without dashes.
        /// </summary>
        [Fact]
        public void NewSessionId_ShouldBe32Hex_NoDashes()
        {
            IdGenerator gen = new IdGenerator();
            string id = gen.NewSessionId();

            Assert.Equal(32, id.Length);
            Regex hex = new Regex("^[0-9a-fA-F]{32}$");
            Assert.Matches(hex, id);
        }
    }
}