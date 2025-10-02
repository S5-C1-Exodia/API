using System.Net;
using System.Text;
using API.DTO;
using API.Errors;
using API.Helpers;
using API.Managers.InterfacesServices;
using Moq;

namespace Tests.Helpers
{
    public class SpotifyOAuthHelperTests
    {
        [Fact]
        public async Task ExchangeCodeForTokensAsync_ShouldReturnTokenInfo_OnSuccess()
        {
            // 1) Prépare deux réponses HTTP :
            //   a) token endpoint
            //   b) /v1/me
            Queue<HttpResponseMessage> responses = new Queue<HttpResponseMessage>();

            string tokenJson =
                "{\"access_token\":\"at\",\"token_type\":\"Bearer\",\"scope\":\"s1 s2\",\"expires_in\":3600,\"refresh_token\":\"rt\"}";
            responses.Enqueue(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(tokenJson, Encoding.UTF8, "application/json")
                }
            );

            string meJson = "{\"id\":\"user123\"}";
            responses.Enqueue(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(meJson, Encoding.UTF8, "application/json")
                }
            );

            FakeHandler handler = new FakeHandler(responses);
            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(15);

            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("spotify-oauth")).Returns(client);

            Mock<IConfigService> cfg = new Mock<IConfigService>();
            cfg.Setup(c => c.GetSpotifyTokenEndpoint()).Returns("https://accounts.spotify.com/api/token");
            cfg.Setup(c => c.GetSpotifyClientId()).Returns("clientId");

            Mock<IClockService> clock = new Mock<IClockService>();
            clock.Setup(c => c.GetUtcNow()).Returns(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));

            Mock<IAuditService> audit = new Mock<IAuditService>();

            SpotifyOAuthHelper helper = new SpotifyOAuthHelper(factory.Object, cfg.Object, clock.Object, audit.Object);

            TokenInfo info = await helper.ExchangeCodeForTokensAsync("code123", "https://cb", "verif");

            Assert.Equal("rt", info.RefreshToken);
            Assert.Equal("at", info.AccessToken);
            Assert.Equal("s1 s2", info.Scope);
            Assert.Equal("user123", info.ProviderUserId);
            Assert.True(info.AccessExpiresAt > clock.Object.GetUtcNow());
        }

        [Fact]
        public async Task ExchangeCodeForTokensAsync_ShouldThrow_OnTokenEndpoint400()
        {
            Queue<HttpResponseMessage> responses = new Queue<HttpResponseMessage>();
            responses.Enqueue(
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{\"error\":\"invalid_grant\"}", Encoding.UTF8, "application/json")
                }
            );

            FakeHandler handler = new FakeHandler(responses);
            HttpClient client = new HttpClient(handler);

            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("spotify-oauth")).Returns(client);

            Mock<IConfigService> cfg = new Mock<IConfigService>();
            cfg.Setup(c => c.GetSpotifyTokenEndpoint()).Returns("https://accounts.spotify.com/api/token");
            cfg.Setup(c => c.GetSpotifyClientId()).Returns("clientId");

            Mock<IClockService> clock = new Mock<IClockService>();
            clock.Setup(c => c.GetUtcNow()).Returns(DateTime.UtcNow);

            Mock<IAuditService> audit = new Mock<IAuditService>();

            SpotifyOAuthHelper helper = new SpotifyOAuthHelper(factory.Object, cfg.Object, clock.Object, audit.Object);

            await Assert.ThrowsAsync<TokenExchangeFailedException>(() =>
                helper.ExchangeCodeForTokensAsync("badcode", "https://cb", "verif")
            );
        }

        [Fact]
        public async Task ExchangeCodeForTokensAsync_ShouldThrow_OnMissingRefreshToken()
        {
            Queue<HttpResponseMessage> responses = new Queue<HttpResponseMessage>();
            // Pas de refresh_token renvoyé
            responses.Enqueue(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"access_token\":\"at\",\"expires_in\":3600,\"scope\":\"s\"}",
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

            FakeHandler handler = new FakeHandler(responses);
            HttpClient client = new HttpClient(handler);

            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("spotify-oauth")).Returns(client);

            Mock<IConfigService> cfg = new Mock<IConfigService>();
            cfg.Setup(c => c.GetSpotifyTokenEndpoint()).Returns("https://accounts.spotify.com/api/token");
            cfg.Setup(c => c.GetSpotifyClientId()).Returns("clientId");

            Mock<IClockService> clock = new Mock<IClockService>();
            clock.Setup(c => c.GetUtcNow()).Returns(DateTime.UtcNow);

            Mock<IAuditService> audit = new Mock<IAuditService>();

            SpotifyOAuthHelper helper = new SpotifyOAuthHelper(factory.Object, cfg.Object, clock.Object, audit.Object);

            await Assert.ThrowsAsync<TokenExchangeFailedException>(() =>
                helper.ExchangeCodeForTokensAsync("code", "https://cb", "verif")
            );
        }

        [Fact]
        public async Task ExchangeCodeForTokensAsync_ShouldThrow_OnProfileError()
        {
            Queue<HttpResponseMessage> responses = new Queue<HttpResponseMessage>();
            // token OK
            responses.Enqueue(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"access_token\":\"at\",\"expires_in\":3600,\"refresh_token\":\"rt\",\"scope\":\"s\"}",
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );
            // /v1/me renvoie erreur
            responses.Enqueue(
                new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                }
            );

            FakeHandler handler = new FakeHandler(responses);
            HttpClient client = new HttpClient(handler);

            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("spotify-oauth")).Returns(client);

            Mock<IConfigService> cfg = new Mock<IConfigService>();
            cfg.Setup(c => c.GetSpotifyTokenEndpoint()).Returns("https://accounts.spotify.com/api/token");
            cfg.Setup(c => c.GetSpotifyClientId()).Returns("clientId");

            Mock<IClockService> clock = new Mock<IClockService>();
            clock.Setup(c => c.GetUtcNow()).Returns(DateTime.UtcNow);

            Mock<IAuditService> audit = new Mock<IAuditService>();

            SpotifyOAuthHelper helper = new SpotifyOAuthHelper(factory.Object, cfg.Object, clock.Object, audit.Object);

            await Assert.ThrowsAsync<TokenExchangeFailedException>(() =>
                helper.ExchangeCodeForTokensAsync("code", "https://cb", "verif")
            );
        }

        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> _responses;

            public FakeHandler(Queue<HttpResponseMessage> responses)
            {
                if (responses == null)
                {
                    throw new ArgumentNullException(nameof(responses));
                }

                this._responses = responses;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (this._responses.Count == 0)
                {
                    HttpResponseMessage empty = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent("No fake responses queued.", Encoding.UTF8, "text/plain")
                    };
                    return Task.FromResult(empty);
                }

                HttpResponseMessage next = this._responses.Dequeue();
                return Task.FromResult(next);
            }
        }
    }
}