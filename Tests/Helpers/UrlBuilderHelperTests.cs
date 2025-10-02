using System.Web;
using API.Helpers;
using API.Managers.InterfacesServices;
using Moq;

namespace Tests.Helpers
{
    public class UrlBuilderHelperTests
    {
        [Fact]
        public void BuildAuthorizeUrl_ShouldComposeAllParameters()
        {
            Mock<IConfigService> cfg = new Mock<IConfigService>();
            cfg.Setup(c => c.GetSpotifyAuthorizeEndpoint()).Returns("https://accounts.spotify.com/authorize");

            UrlBuilderHelper helper = new UrlBuilderHelper(cfg.Object);

            string[] scopes = ["user-read-email", "playlist-read-private"];
            string url = helper.BuildAuthorizeUrl(
                "client_123",
                "https://example.com/callback",
                scopes,
                "state_abc",
                "challenge_xyz",
                "S256"
            );

            Uri uri = new Uri(url);
            Assert.Equal("https", uri.Scheme);
            Assert.Equal("accounts.spotify.com", uri.Host);
            Assert.Equal("/authorize", uri.AbsolutePath);

            var q = HttpUtility.ParseQueryString(uri.Query);
            Assert.Equal("client_123", q["client_id"]);
            Assert.Equal("code", q["response_type"]);
            Assert.Equal("https://example.com/callback", q["redirect_uri"]);
            Assert.Equal("user-read-email playlist-read-private", q["scope"]);
            Assert.Equal("state_abc", q["state"]);
            Assert.Equal("challenge_xyz", q["code_challenge"]);
            Assert.Equal("S256", q["code_challenge_method"]);
        }

        [Fact]
        public void BuildAuthorizeUrl_ShouldThrow_OnInvalidInputs()
        {
            Mock<IConfigService> cfg = new Mock<IConfigService>();
            cfg.Setup(c => c.GetSpotifyAuthorizeEndpoint()).Returns("https://accounts.spotify.com/authorize");
            UrlBuilderHelper helper = new UrlBuilderHelper(cfg.Object);

            Assert.Throws<ArgumentException>(() => helper.BuildAuthorizeUrl("", "cb", ["a"], "s", "c", "S256"));
            Assert.Throws<ArgumentException>(() => helper.BuildAuthorizeUrl("id", "", ["a"], "s", "c", "S256"));
            Assert.Throws<ArgumentException>(() => helper.BuildAuthorizeUrl("id", "cb", [], "s", "c", "S256"));
            Assert.Throws<ArgumentException>(() => helper.BuildAuthorizeUrl("id", "cb", ["a"], "", "c", "S256"));
            Assert.Throws<ArgumentException>(() => helper.BuildAuthorizeUrl("id", "cb", ["a"], "s", "", "S256"));
            Assert.Throws<ArgumentException>(() => helper.BuildAuthorizeUrl("id", "cb", ["a"], "s", "c", ""));
        }
    }
}