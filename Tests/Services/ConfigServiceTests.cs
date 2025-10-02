using API.Services;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="ConfigService"/>.
    /// </summary>
    public class ConfigServiceTests
    {
        /// <summary>
        /// Tests that the getters of <see cref="ConfigService"/> return the values provided to the constructor.
        /// </summary>
        [Fact]
        public void Getters_ShouldReturnConstructorValues()
        {
            ConfigService cfg = new ConfigService(
                "clientId",
                "https://cb",
                "https://accounts.spotify.com/authorize",
                "https://accounts.spotify.com/api/token",
                "swipez://oauth-callback/spotify",
                10,
                60
            );

            Assert.Equal("clientId", cfg.GetSpotifyClientId());
            Assert.Equal("https://cb", cfg.GetSpotifyRedirectUri());
            Assert.Equal("https://accounts.spotify.com/authorize", cfg.GetSpotifyAuthorizeEndpoint());
            Assert.Equal("https://accounts.spotify.com/api/token", cfg.GetSpotifyTokenEndpoint());
            Assert.Equal("swipez://oauth-callback/spotify", cfg.GetDeeplinkSchemeHost());
            Assert.Equal(10, cfg.GetPkceTtlMinutes());
            Assert.Equal(60, cfg.GetSessionTtlMinutes());
        }

        /// <summary>
        /// Tests that the <see cref="ConfigService"/> constructor throws an <see cref="ArgumentException"/> for invalid arguments.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_OnInvalidArgs()
        {
            Assert.Throws<ArgumentException>(() => new ConfigService(
                    "",
                    "cb",
                    "auth",
                    "token",
                    "scheme",
                    10,
                    60
                )
            );

            Assert.Throws<ArgumentException>(() => new ConfigService(
                    "id",
                    "",
                    "auth",
                    "token",
                    "scheme",
                    10,
                    60
                )
            );

            Assert.Throws<ArgumentException>(() => new ConfigService(
                    "id",
                    "cb",
                    "",
                    "token",
                    "scheme",
                    10,
                    60
                )
            );

            Assert.Throws<ArgumentException>(() => new ConfigService(
                    "id",
                    "cb",
                    "auth",
                    "",
                    "scheme",
                    10,
                    60
                )
            );

            Assert.Throws<ArgumentException>(() => new ConfigService(
                    "id",
                    "cb",
                    "auth",
                    "token",
                    "",
                    10,
                    60
                )
            );

            Assert.Throws<ArgumentException>(() => new ConfigService(
                    "id",
                    "cb",
                    "auth",
                    "token",
                    "scheme",
                    0,
                    60
                )
            );

            Assert.Throws<ArgumentException>(() => new ConfigService(
                    "id",
                    "cb",
                    "auth",
                    "token",
                    "scheme",
                    10,
                    0
                )
            );
        }
    }
}