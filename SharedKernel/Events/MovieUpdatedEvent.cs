namespace SharedKernel.Events;

public class MovieUpdatedEvent : DomainEvent
{
    public int MovieId { get; }
    public string Title { get; }
    public string Description { get; }
    public DateTime UpdatedAt { get; }
    public string GenreName { get; }
    
    public int Id => MovieId;

    public MovieUpdatedEvent(int movieId, string title, string description, string genreName, DateTime updatedAt)
    {
        MovieId = movieId;
        Title = title;
        UpdatedAt = DateTime.UtcNow;
        Description = description;
        GenreName = genreName;
    }
}