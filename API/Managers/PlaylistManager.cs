using API.Controllers.InterfacesManagers;
using API.DTO;
using API.Managers.InterfacesHelpers;
using API.Services;
using Microsoft.Extensions.Caching.Memory;

namespace API.Managers;

public class PlaylistManager : IPlaylistManager
{
    private readonly ITokenService _tokenService;
    private readonly ISpotifyApiHelper _spotifyApiHelper;
    private readonly IMemoryCache _memoryCache;

    public PlaylistManager(ITokenService tokenService, ISpotifyApiHelper spotifyApiHelper, IMemoryCache memoryCache)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _spotifyApiHelper = spotifyApiHelper ?? throw new ArgumentNullException(nameof(spotifyApiHelper));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<PlaylistTracksDTO> GetTracksByPlaylist(
        string sessionId, 
        string playlistId, 
        int? offset, 
        CancellationToken ct = default)
    {
        string cacheKey = $"playlist_offset_{sessionId}_{playlistId}";
        int currentOffset = offset ?? _memoryCache.Get<int?>(cacheKey) ?? 0;
        string accessToken = await _tokenService.GetAccessTokenAsync(sessionId, ct);
        PlaylistTracksDTO res = await _spotifyApiHelper.GetPlaylistTracks(accessToken, playlistId, currentOffset, ct);

        int nextOffset = currentOffset + res.Limit;

        _memoryCache.Set(cacheKey, nextOffset, TimeSpan.FromDays(2));

        res.Offset = currentOffset;
        res.NextOffset = nextOffset;
        
        return res;
    }

}