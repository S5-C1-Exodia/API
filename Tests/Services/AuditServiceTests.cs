using API.Services;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="AuditService"/>.
    /// </summary>
    public class AuditServiceTests
    {
        /// <summary>
        /// Tests that <see cref="AuditService.LogAuth(string, string, string)"/> writes the expected output to the console.
        /// </summary>
        [Fact]
        public void LogAuth_ShouldWriteToConsole()
        {
            AuditService audit = new AuditService();

            StringWriter sw = new StringWriter();
            TextWriter original = Console.Out;
            Console.SetOut(sw);

            try
            {
                audit.LogAuth("spotify", "AuthSuccess", "UserId=abc");
            }
            finally
            {
                Console.SetOut(original);
            }

            string output = sw.ToString();
            Assert.Contains("provider=spotify", output);
            Assert.Contains("action=AuthSuccess", output);
            Assert.Contains("UserId=abc", output);
        }
    }
}