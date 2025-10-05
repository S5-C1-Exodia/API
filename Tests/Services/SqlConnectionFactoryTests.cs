using API.Services;
using MySqlConnector;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="SqlConnectionFactory"/>.
    /// </summary>
    public class SqlConnectionFactoryTests
    {
        [Fact]
        public void Ctor_ShouldThrow_OnEmptyConnectionString()
        {
            Assert.Throws<ArgumentException>(() => new SqlConnectionFactory(""));
        }

        [Fact]
        public void Create_ShouldReturnMySqlConnection()
        {
            var factory = new SqlConnectionFactory("Server=localhost;Uid=root;Pwd=pw;Database=test;");
            MySqlConnection conn = factory.Create();
            Assert.NotNull(conn);
            Assert.IsType<MySqlConnection>(conn);
        }
    }
}