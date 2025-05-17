
using SharedKernel.Events;

namespace MovieService.Domain.Entities
{
    public class Movie : Entity
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public decimal Rating { get; set; }
        public int GenreId { get; set; }
        public required Genre Genre { get; set; }
        public required string VideoUrl { get; set; }
        public required string PosterUrl { get; set; }
        
        public void AddCreatedEvent()
        {
            AddDomainEvent(new MovieCreatedEvent(
                Id,
                Title,
                Description,
                ReleaseDate,
                Rating,
                Genre?.Name ?? string.Empty,
                VideoUrl,
                PosterUrl
            ));
        }
        
        public void AddUpdatedEvent()
        {
            AddDomainEvent(new MovieUpdatedEvent(
                Id,
                Title,
                Description,
                ReleaseDate,
                Rating,
                Genre?.Name ?? string.Empty,
                VideoUrl,
                PosterUrl
            ));
        }
        
        public void AddDeletedEvent()
        {
            AddDomainEvent(new MovieDeletedEvent(Id));
        }
        public void UpdateRatingIfChanged(decimal newRating)
        {
            if (Rating != newRating)
            {
                Rating = newRating;
            }
        }
    }
}