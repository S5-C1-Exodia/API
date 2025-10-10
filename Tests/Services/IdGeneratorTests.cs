using API.Services;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="IdGenerator"/>.
    /// </summary>
    public class IdGeneratorTests
    {
        [Fact]
        public void NewSessionId_ShouldReturnUnique32Hex()
        {
            var gen = new IdGenerator();

            string id1 = gen.NewSessionId();
            string id2 = gen.NewSessionId();

            Assert.NotNull(id1);
            Assert.NotNull(id2);
            Assert.Equal(32, id1.Length);
            Assert.Equal(32, id2.Length);
            Assert.Matches("^[0-9a-f]{32}$", id1);
            Assert.NotEqual(id1, id2);
        }
    }
}