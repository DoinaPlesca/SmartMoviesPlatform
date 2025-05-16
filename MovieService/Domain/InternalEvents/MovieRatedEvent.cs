using SharedKernel.Events;

namespace MovieService.Domain.InternalEvents;

public class MovieRatedEvent : DomainEvent
{
    public int Id { get; }
    public decimal NewRating { get; }

    public MovieRatedEvent(int id, decimal newRating)
    {
        Id = id;
        NewRating = newRating;
    }
}