using System.Net.Http.Headers;
using System.Text.Json;
using API.DTO;
using API.Managers.InterfacesHelpers;
using API.Managers.InterfacesServices;

namespace API.Helpers;

/// <summary>
/// Helper class for interacting with the Spotify Web API.
/// Provides methods to retrieve playlists for a user.
/// </summary>
public class SpotifyApiHelper(HttpClient http, IConfigService config) : ISpotifyApiHelper
{
    private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly IConfigService _config = config ?? throw new ArgumentNullException(nameof(config));

    /// <inheritdoc />
    public async Task<PlaylistPageDto> GetPlaylistsAsync(string accessToken, string? pageToken,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("accessToken cannot be null or empty.", nameof(accessToken));

        string url;

        if (!string.IsNullOrWhiteSpace(pageToken) && Uri.IsWellFormedUriString(pageToken, UriKind.Absolute))
        {
            url = pageToken;
        }
        else
        {
            int limit = _config.GetSpotifyPlaylistsPageSize();
            string offsetParam = string.Empty;
            if (!string.IsNullOrWhiteSpace(pageToken) && int.TryParse(pageToken, out int offset) && offset >= 0)
                offsetParam = $"&offset={offset}";

            url = $"me/playlists?limit={limit}{offsetParam}";
        }

        using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using HttpResponseMessage resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        SpotifyPlaylistsResponse? json = await JsonSerializer.DeserializeAsync<SpotifyPlaylistsResponse>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            ct
        );

        if (json is null)
            throw new InvalidOperationException("Failed to deserialize Spotify playlists response.");

        PlaylistPageDto dto = new PlaylistPageDto
        {
            Items = json.Items.Select(i => new PlaylistItemDto
                {
                    PlaylistId = i.Id ?? string.Empty,
                    Name = i.Name ?? string.Empty,
                    ImageUrl = i.Images?.FirstOrDefault()?.Url,
                    Owner = i.Owner?.DisplayName ?? i.Owner?.Id,
                    TrackCount = i.Tracks?.Count,
                    Selected = false
                }
            ).ToList(),
            NextPageToken = json.Next
        };

        return dto;
    }

    /// <inheritdoc/>
    public async Task<SpotifyPlaylistItem> GetPlaylistTracks(string accessToken, string playlistId, int? offset, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("accessToken cannot be null or empty.", nameof(accessToken));
        if (string.IsNullOrWhiteSpace(playlistId))
            throw new ArgumentException("playlistId cannot be null or empty.", nameof(playlistId));

        string url = $"playlists/{playlistId}/tracks?limit={_config.GetSpotifyPlaylistsPageSize()}&offset={offset}";

        using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using HttpResponseMessage resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var spotifyResponse = await JsonSerializer.DeserializeAsync<PlaylistTracksResponse>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            ct
        );

        if (spotifyResponse == null)
            throw new InvalidOperationException("Failed to deserialize Spotify playlist tracks response.");

        List<SpotifyTrack> tracks = spotifyResponse.Items
            .Where(i => i.Track != null)
            .Select(i => new SpotifyTrack
            {
                Id = i.Track.Id ?? string.Empty,
                Name = i.Track.Name ?? string.Empty,
                Artists = i.Track.Artists?.Select(a => new ArtistDTO
                {
                    Id = a.Id,
                    Name = a.Name
                }).ToList() ?? new List<ArtistDTO>(),
                Album = i.Track.Album != null
                    ? new AlbumDTO
                    {
                        Id = i.Track.Album.Id,
                        Images = i.Track.Album.Images?
                            .Select(img => new SpotifyImage() { Url = img.Url })
                            .ToList() ?? new List<SpotifyImage>()
                    }
                    : throw new NullReferenceException("None album for this track")
            }).ToList();

        return new SpotifyPlaylistItem()
        {
            Id = playlistId,
            Limit = spotifyResponse.Limit,
            Offset = spotifyResponse.Offset,
            Tracks = tracks
        };
    }
}