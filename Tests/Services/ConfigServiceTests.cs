using API.Services;

namespace Tests.Services;

public class ConfigServiceTests
{

    [Fact]
    public void Getters_ShouldReturnConstructorValues()
    {
        ConfigService cfg = new ConfigService(
            "https://api.spotify.com",
            "clientId",
            "https://cb",
            "https://accounts.spotify.com/authorize",
            "https://accounts.spotify.com/api/token",
            10,
            60,
            "swipez://oauth-callback/spotify",
            15,
            120
        );

        Assert.Equal("clientId", cfg.GetSpotifyClientId());
        Assert.Equal("https://cb", cfg.GetSpotifyRedirectUri());
        Assert.Equal("https://accounts.spotify.com/authorize", cfg.GetSpotifyAuthorizeEndpoint());
        Assert.Equal("https://accounts.spotify.com/api/token", cfg.GetSpotifyTokenEndpoint());
        Assert.Equal("swipez://oauth-callback/spotify", cfg.GetDeeplinkSchemeHost());
        Assert.Equal(15, cfg.GetPkceTtlMinutes());
        Assert.Equal(120, cfg.GetSessionTtlMinutes());
    }
    
    [Fact]
    public void Constructor_ShouldThrow_OnInvalidArgs()
    {
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "",
                "cb",
                "auth",
                "token",
                10,
                60,
                "scheme",
                15,
                120
            )
        );
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "id",
                "",
                "auth",
                "token",
                10,
                60,
                "scheme",
                15,
                120
            )
        );
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "id",
                "cb",
                "",
                "token",
                10,
                60,
                "scheme",
                15,
                120
            )
        );
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "id",
                "cb",
                "auth",
                "",
                10,
                60,
                "scheme",
                15,
                120
            )
        );
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "id",
                "cb",
                "auth",
                "token",
                10,
                60,
                "",
                15,
                120
            )
        );
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "id",
                "cb",
                "auth",
                "token",
                10,
                60,
                "scheme",
                0,
                120
            )
        );
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "id",
                "cb",
                "auth",
                "token",
                10,
                60,
                "scheme",
                15,
                0
            )
        );
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "id",
                "cb",
                "auth",
                "token",
                0,
                60,
                "scheme",
                15,
                120
            )
        );
        Assert.Throws<ArgumentException>(() => new ConfigService(
                "https://api.spotify.com",
                "id",
                "cb",
                "auth",
                "token",
                10,
                0,
                "scheme",
                15,
                120
            )
        );
    }
}