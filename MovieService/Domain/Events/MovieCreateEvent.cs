using MovieService.Domain.Entities;

namespace MovieService.Domain.Events;

public class MovieCreatedEvent : DomainEvent
{
    public int Id { get; }
    public string Title { get; }
    public string Description { get; }
    public DateTime ReleaseDate { get; }
    public decimal Rating { get; }
    public string GenreName { get; }
    public string VideoUrl { get; }
    public string PosterUrl { get; }

    public MovieCreatedEvent(Movie movie)
    {
        Id = movie.Id;
        Title = movie.Title;
        Description = movie.Description;
        ReleaseDate = movie.ReleaseDate;
        Rating = movie.Rating;
        GenreName = movie.Genre.Name;
        VideoUrl = movie.VideoUrl;
        PosterUrl = movie.PosterUrl;
    }
}