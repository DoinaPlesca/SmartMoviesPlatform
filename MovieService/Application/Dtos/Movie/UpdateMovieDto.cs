namespace MovieService.Application.Dtos.Movie;

public class UpdateMovieDto
{
    public int Id { get; set; } 

    public string Title { get; set; } = null!;
    public required string Description { get; set; }
    public DateTime ReleaseDate { get; set; }
    public decimal Rating { get; set; }
    public int GenreId { get; set; }
   
    public IFormFile? PosterFile { get; set; }
    public IFormFile? VideoFile { get; set; }
}