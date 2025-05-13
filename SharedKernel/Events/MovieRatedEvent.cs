namespace SharedKernel.Events;

public class MovieRatedEvent : DomainEvent
{
    public int MovieId { get; }
    public decimal NewRating { get; }

    public MovieRatedEvent(int movieId, decimal newRating)
    {
        MovieId = movieId;
        NewRating = newRating;
    }
}