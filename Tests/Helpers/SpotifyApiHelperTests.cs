using System.Net;
using System.Text.Json;
using API.DTO;
using API.Helpers;
using API.Managers.InterfacesServices;
using Moq;
using Moq.Protected;

namespace Tests.Helpers;

public class SpotifyApiHelperTests
{
    private readonly Mock<IConfigService> _config = new Mock<IConfigService>();
    private readonly SpotifyApiHelper _helper;

    public SpotifyApiHelperTests()
    {
        var handler = new Mock<HttpMessageHandler>();
        HttpClient httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        _config.Setup(c => c.GetSpotifyPlaylistsPageSize()).Returns(20);
        _helper = new SpotifyApiHelper(httpClient, _config.Object);
    }

    [Fact]
    public async Task GetPlaylistsAsync_Throws_OnEmptyAccessToken()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _helper.GetPlaylistsAsync("", null));
    }

    [Fact]
    public async Task GetPlaylistsAsync_ReturnsPlaylists_OnValidResponse()
    {
        var responseObj = new SpotifyPlaylistsResponse
        {
            Items =
            [
                new SpotifyPlaylistItem
                {
                    Id = "id1",
                    Name = "Playlist 1",
                    Images = new[] { new SpotifyImage { Url = "img1" } },
                    Owner = new SpotifyOwner { DisplayName = "owner1" },
                    Tracks = new SpotifyTracks { Total = 10 }
                }
            ],
            Next = "next-token"
        };
        var json = JsonSerializer.Serialize(responseObj);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                }
            );
        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);
        var result = await helper.GetPlaylistsAsync("token", null);
        Assert.Single(result.Items);
        Assert.Equal("id1", result.Items[0].PlaylistId);
        Assert.Equal("next-token", result.NextPageToken);
    }

    [Fact]
    public async Task GetPlaylistsAsync_Throws_OnInvalidJson()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("not a json")
                }
            );
        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => helper.GetPlaylistsAsync("token", null));
    }
}