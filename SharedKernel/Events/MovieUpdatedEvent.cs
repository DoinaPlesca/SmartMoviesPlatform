namespace SharedKernel.Events;
public class MovieUpdatedEvent : DomainEvent
{
    public int Id { get; }
    public string Title { get; }
    public string Description { get; }
    public DateTime ReleaseDate { get; }
    public decimal Rating { get; }
    public string GenreName { get; }
    public string VideoUrl { get; }
    public string PosterUrl { get; }

    public MovieUpdatedEvent(
        int id,
        string title,
        string description,
        DateTime releaseDate,
        decimal rating,
        string genreName,
        string videoUrl,
        string posterUrl)
    {
        Id = id;
        Title = title;
        Description = description;
        ReleaseDate = releaseDate;
        Rating = rating;
        GenreName = genreName;
        VideoUrl = videoUrl;
        PosterUrl = posterUrl;
    }
}
