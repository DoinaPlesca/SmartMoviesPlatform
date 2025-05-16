using SharedKernel.Events;

namespace MovieService.Domain.InternalEvents;

public class PosterReplacedEvent : DomainEvent
{
    public int Id { get; }
    public string NewPosterUrl { get; }

    public PosterReplacedEvent(int id, string newPosterUrl)
    {
        Id = id;
        NewPosterUrl = newPosterUrl;
    }
}