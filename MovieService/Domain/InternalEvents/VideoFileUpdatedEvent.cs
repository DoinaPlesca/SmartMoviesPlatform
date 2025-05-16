using SharedKernel.Events;

namespace MovieService.Domain.InternalEvents;

public class VideoFileUpdatedEvent : DomainEvent
{
    public int Id { get; }
    public string NewVideoUrl { get; }

    public VideoFileUpdatedEvent(int id, string newVideoUrl)
    {
       Id = id;
        NewVideoUrl = newVideoUrl;
    }
}