
using MovieService.Domain.InternalEvents;
using SharedKernel.Events;

namespace MovieService.Domain.Entities
{
    public class Movie : Entity
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime ReleaseDate { get; set; }

        public decimal Rating { get; set; }

        public int GenreId { get; set; }

        public Genre Genre { get; set; } = null!;

        public string VideoUrl { get; set; } = null!;

        public string PosterUrl { get; set; } = null!;
        
        
        //  Shared event
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
                Genre?.Name ?? string.Empty,
                ReleaseDate
            ));
        }


        public void AddDeletedEvent()
        {
            AddDomainEvent(new MovieDeletedEvent(Id));
        }
        
        //  Internal events (used only inside MovieService)
        public void AddPosterReplacedEvent()
        {
            AddDomainEvent(new PosterReplacedEvent(Id, PosterUrl));
        }
        
        public void AddVideoFileUpdatedEvent()
        {
            AddDomainEvent(new VideoFileUpdatedEvent(Id, VideoUrl));
        }
        
        public void AddRatedEvent()
        {
            AddDomainEvent(new MovieRatedEvent(Id, Rating));
        }
        
        public void UpdateRatingIfChanged(decimal newRating)
        {
            if (Rating != newRating)
            {
                Rating = newRating;
                AddRatedEvent();
            }
        }

    }
}