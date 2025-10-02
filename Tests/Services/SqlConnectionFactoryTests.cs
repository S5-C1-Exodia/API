using API.Services;
using MySqlConnector;

namespace Tests.Services
{
    public class SqlConnectionFactoryTests
    {
        [Fact]
        public void Constructor_ShouldThrow_OnEmptyConnectionString()
        {
            Assert.Throws<ArgumentException>(() => new SqlConnectionFactory(""));
            Assert.Throws<ArgumentException>(() => new SqlConnectionFactory("   "));
        }

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