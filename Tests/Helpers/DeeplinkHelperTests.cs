using API.Helpers;
using API.Managers.InterfacesServices;
using Moq;

namespace Tests.Helpers
{
    public class DeeplinkHelperTests
    {
        [Fact]
        public void BuildDeepLink_ShouldComposeSchemeAndSid()
        {
            Mock<IConfigService> cfg = new Mock<IConfigService>();
            cfg.Setup(c => c.GetDeeplinkSchemeHost()).Returns("swipez://oauth-callback/spotify");

            DeeplinkHelper helper = new DeeplinkHelper(cfg.Object);

            string link = helper.BuildDeepLink("SID_123");

            Assert.StartsWith("swipez://oauth-callback/spotify", link);
            Assert.Contains("?sid=SID_123", link);
        }

        [Fact]
        public void BuildDeepLink_ShouldThrow_OnInvalidSid()
        {
            Mock<IConfigService> cfg = new Mock<IConfigService>();
            cfg.Setup(c => c.GetDeeplinkSchemeHost()).Returns("swipez://oauth-callback/spotify");

            DeeplinkHelper helper = new DeeplinkHelper(cfg.Object);

            Assert.Throws<ArgumentException>(() => helper.BuildDeepLink(""));
            Assert.Throws<ArgumentException>(() => helper.BuildDeepLink("   "));
        }
    }
}