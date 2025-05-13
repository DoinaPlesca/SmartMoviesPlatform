namespace MovieService.Domain.Events;

public class PosterReplacedEvent : DomainEvent
{
    public int MovieId { get; }
    public string NewPosterUrl { get; }

    public PosterReplacedEvent(int movieId, string newPosterUrl)
    {
        MovieId = movieId;
        NewPosterUrl = newPosterUrl;
    }
}