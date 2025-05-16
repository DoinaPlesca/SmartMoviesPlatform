namespace SharedKernel.Events;

public class MovieDeletedEvent : DomainEvent
{
    public int Id { get; }

    public MovieDeletedEvent(int id)
    {
        Id = id;
    }

}