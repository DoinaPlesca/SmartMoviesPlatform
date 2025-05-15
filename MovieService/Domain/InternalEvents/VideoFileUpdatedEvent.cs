using SharedKernel.Events;

namespace MovieService.Domain.InternalEvents;

public class VideoFileUpdatedEvent : DomainEvent
{
    public int MovieId { get; }
    public string NewVideoUrl { get; }

    public VideoFileUpdatedEvent(int movieId, string newVideoUrl)
    {
        MovieId = movieId;
        NewVideoUrl = newVideoUrl;
    }
}