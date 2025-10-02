using API.Services;
using MySqlConnector;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="SqlConnectionFactory"/>.
    /// </summary>
    public class SqlConnectionFactoryTests
    {
        /// <summary>
        /// Tests that the <see cref="SqlConnectionFactory"/> constructor throws an <see cref="ArgumentException"/> for an empty connection string.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_OnEmptyConnectionString()
        {
            Assert.Throws<ArgumentException>(() => new SqlConnectionFactory(""));
            Assert.Throws<ArgumentException>(() => new SqlConnectionFactory("   "));
        }

        /// <summary>
        /// Tests that <see cref="SqlConnectionFactory.Create"/> returns a valid <see cref="MySqlConnection"/> instance.
        /// </summary>
        [Fact]
        public void Create_ShouldReturn_MySqlConnection_Instance()
        {
            string cs = "Server=localhost;Port=3306;Database=test;Uid=root;Pwd=pwd;";
            SqlConnectionFactory factory = new SqlConnectionFactory(cs);

            MySqlConnection conn = factory.Create();

            Assert.NotNull(conn);
            Assert.IsType<MySqlConnection>(conn);
            // Ne pas ouvrir la connexion ici (test unitaire pur)
        }
    }
}