namespace MovieService.Application.DTOs;

public class MovieDto
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; }

    public DateTime ReleaseDate { get; set; }

    public decimal Rating { get; set; }

    public string GenreName { get; set; }

    public string VideoUrl { get; set; }

    public string PosterUrl { get; set; }
}