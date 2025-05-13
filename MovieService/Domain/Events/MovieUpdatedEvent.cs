using MovieService.Domain.Entities;

namespace MovieService.Domain.Events;

public class MovieUpdatedEvent : DomainEvent
{
    public int MovieId { get; }
    public string Title { get; }
    public DateTime UpdatedAt { get; }

    public MovieUpdatedEvent(Movie movie)
    {
        MovieId = movie.Id;
        Title = movie.Title;
        UpdatedAt = DateTime.UtcNow;
    }
}