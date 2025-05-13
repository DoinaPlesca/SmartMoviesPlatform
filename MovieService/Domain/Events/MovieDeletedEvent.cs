namespace MovieService.Domain.Events;

public class MovieDeletedEvent : DomainEvent
{
    public int MovieId { get; }

    public MovieDeletedEvent(int movieId)
    {
        MovieId = movieId;
    }
}