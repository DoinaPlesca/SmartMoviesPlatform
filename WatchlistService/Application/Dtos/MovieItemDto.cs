namespace WatchlistService.Application.Dtos;

public class MovieItemDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public DateTime ReleaseDate { get; set; }
    public decimal Rating { get; set; }
    public string PosterUrl { get; set; } = null!;
    public string VideoUrl { get; set; } = null!;
    
    public Dictionary<string, object>? ExtraFields { get; set; }
}
