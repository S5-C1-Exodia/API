using API.Controllers.InterfacesManagers;
using API.DTO;
using API.Managers.InterfacesHelpers;
using API.Services;

namespace API.Managers;

public class PlaylistManager : IPlaylistManager
{
    private readonly ITokenService _tokenService;
    private readonly ISpotifyApiHelper _spotifyApiHelper;

    public PlaylistManager(ITokenService tokenService, ISpotifyApiHelper spotifyApiHelper)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _spotifyApiHelper = spotifyApiHelper ?? throw new ArgumentNullException(nameof(spotifyApiHelper));
    }

    public async Task<PlaylistTracksDTO> GetTracksByPlaylist(string sessionId, string playlistId, CancellationToken ct = default)
    {
        string accessToken = await _tokenService.GetAccessTokenAsync(sessionId, ct);
        return await _spotifyApiHelper.GetPlaylistTracks(accessToken, playlistId, ct);
    }

}