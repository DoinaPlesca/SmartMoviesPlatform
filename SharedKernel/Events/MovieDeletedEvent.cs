namespace SharedKernel.Events;

public class MovieDeletedEvent : DomainEvent
{
    public int Id { get; }

    public MovieDeletedEvent(int id)
    {
        if (id <= 0)
            throw new ArgumentException("MovieDeletedEvent must have a valid ID.");

        Id = id;
    }

}