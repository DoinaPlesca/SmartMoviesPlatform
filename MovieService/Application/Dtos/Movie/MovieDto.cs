namespace MovieService.Application.Dtos.Movie;

public class MovieDto
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public required string Description { get; set; }

    public DateTime ReleaseDate { get; set; }

    public decimal Rating { get; set; }

    public required string GenreName { get; set; }

    public required string VideoUrl { get; set; }

    public required string PosterUrl { get; set; }
}