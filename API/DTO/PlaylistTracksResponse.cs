namespace API.DTO;

public class PlaylistTracksResponse
{
    public List<TrackItemDTO> Items { get; set; }
    public int Limit { get; set; }

    public PlaylistTracksResponse()
    {
        Limit = 20;
    }
}