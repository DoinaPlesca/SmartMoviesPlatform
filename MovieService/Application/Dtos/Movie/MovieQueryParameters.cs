namespace MovieService.Application.Dtos.Movie;

public class MovieQueryParameters
{
    public string? Search { get; set; }
    
    public string? SortBy { get; set; } = "title"; // opt: "title", "rating", "releaseDate", "newest"

    public bool SortDescending { get; set; } = false;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}