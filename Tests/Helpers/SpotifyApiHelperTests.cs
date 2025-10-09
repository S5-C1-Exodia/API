﻿using System.Net;
using System.Text;
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
                    TracksNumber = new SpotifyTracks
                    {
                        Total = 10
                    }
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
        await Assert.ThrowsAsync<JsonException>(() => helper.GetPlaylistsAsync("token", null));
    }

    [Fact]
    public async Task GetPlaylistTracks_ValidRequest_ReturnsPlaylistTracksDTO()
    {
        // Arrange
        var accessToken = "valid_token";
        var playlistId = "playlist123";
        var offset = 0;

        var spotifyResponse = new PlaylistTracksResponse
        {
            Limit = 20,
            Offset = 0,
            Items =
            [
                new SpotifyTrackItem()
                {
                    Track = new SpotifyTrack
                    {
                        Id = "track1",
                        Name = "Test Song",
                        Artists =
                        [
                            new ArtistDTO { Id = "artist1", Name = "Test Artist" }
                        ],
                        Album = new AlbumDTO
                        {
                            Id = "album1",
                            Images =
                            [
                                new SpotifyImage() { Url = "https://example.com/image.jpg" }
                            ]
                        }
                    }
                }
            ]
        };

        var json = JsonSerializer.Serialize(spotifyResponse);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);

        // Act
        var result = await helper.GetPlaylistTracks(accessToken, playlistId, offset);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(playlistId, result.Id);
        Assert.Equal(20, result.Limit);
        Assert.Equal(0, result.Offset);
        Assert.Single(result.Tracks);
        Assert.Equal("track1", result.Tracks[0].Id);
        Assert.Equal("Test Song", result.Tracks[0].Name);
        Assert.Single(result.Tracks[0].Artists);
        Assert.Equal("artist1", result.Tracks[0].Artists[0].Id);
        Assert.NotNull(result.Tracks[0].Album);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPlaylistTracks_InvalidAccessToken_ThrowsArgumentException(string accessToken)
    {
        // Arrange
        var playlistId = "playlist123";

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(() => _helper.GetPlaylistTracks(accessToken, playlistId, 0)
            );
        Assert.Equal("accessToken", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPlaylistTracks_InvalidPlaylistId_ThrowsArgumentException(string playlistId)
    {
        // Arrange
        var accessToken = "valid_token";

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(() => _helper.GetPlaylistTracks(accessToken, playlistId, 0)
            );
        Assert.Equal("playlistId", exception.ParamName);
    }

    [Fact]
    public async Task GetPlaylistTracks_HttpRequestFails_ThrowsHttpRequestException()
    {
        // Arrange
        var accessToken = "valid_token";
        var playlistId = "playlist123";

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => helper.GetPlaylistTracks(accessToken, playlistId, 0)
        );
    }

    [Fact]
    public async Task GetPlaylistTracks_NullResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var accessToken = "valid_token";
        var playlistId = "playlist123";

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null")
            });

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                helper.GetPlaylistTracks(accessToken, playlistId, 0)
            );
        Assert.Contains("Failed to deserialize", exception.Message);
    }

    [Fact]
    public async Task GetPlaylistTracks_TracksWithNullFields_HandlesGracefully()
    {
        // Arrange
        var accessToken = "valid_token";
        var playlistId = "playlist123";

        var spotifyResponse = new PlaylistTracksResponse
        {
            Limit = 20,
            Offset = 0,
            Items =
            [
                new SpotifyTrackItem()
                {
                    Track = new SpotifyTrack
                    {
                        Id = null,
                        Name = null,
                        Artists = null,
                        Album = null
                    }
                }
            ]
        };

        var json = JsonSerializer.Serialize(spotifyResponse);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);

        // Act
        var result = await helper.GetPlaylistTracks(accessToken, playlistId, 0);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tracks);
        Assert.Equal(string.Empty, result.Tracks[0].Id);
        Assert.Equal(string.Empty, result.Tracks[0].Name);
        Assert.Empty(result.Tracks[0].Artists);
        Assert.Null(result.Tracks[0].Album);
    }

    [Fact]
    public async Task GetPlaylistTracks_ItemsWithNullTrack_FiltersOut()
    {
        // Arrange
        var accessToken = "valid_token";
        var playlistId = "playlist123";

        var spotifyResponse = new PlaylistTracksResponse
        {
            Limit = 20,
            Offset = 0,
            Items =
            [
                new SpotifyTrackItem() { Track = null },
                new SpotifyTrackItem()
                {
                    Track = new SpotifyTrack
                    {
                        Id = "track1",
                        Name = "Valid Track",
                        Artists = [],
                        Album = null
                    }
                }
            ]
        };

        var json = JsonSerializer.Serialize(spotifyResponse);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);

        // Act
        var result = await helper.GetPlaylistTracks(accessToken, playlistId, 0);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tracks);
        Assert.Equal("track1", result.Tracks[0].Id);
    }

    [Fact]
    public async Task GetPlaylistTracks_CancellationTokenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var accessToken = "valid_token";
        var playlistId = "playlist123";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException());

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            helper.GetPlaylistTracks(accessToken, playlistId, 0, cts.Token)
        );
    }

    [Fact]
    public async Task GetPlaylistTracks_CorrectUrlAndHeaders_AreSent()
    {
        // Arrange
        var accessToken = "test_bearer_token";
        var playlistId = "playlist456";
        var offset = 20;

        var spotifyResponse = new PlaylistTracksResponse
        {
            Limit = 20,
            Offset = 20,
            Items = []
        };

        HttpRequestMessage capturedRequest = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    JsonSerializer.Serialize(spotifyResponse),
                    Encoding.UTF8,
                    "application/json"
                )
            });

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/v1/")
        };
        var helper = new SpotifyApiHelper(client, _config.Object);

        // Act
        await helper.GetPlaylistTracks(accessToken, playlistId, offset);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest.Method);
        Assert.Contains($"playlists/{playlistId}/tracks", capturedRequest.RequestUri.ToString());
        Assert.Contains("limit=20", capturedRequest.RequestUri.ToString());
        Assert.Contains("offset=20", capturedRequest.RequestUri.ToString());
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal(accessToken, capturedRequest.Headers.Authorization.Parameter);
    }
}