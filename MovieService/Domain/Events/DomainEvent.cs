using System.ComponentModel.DataAnnotations.Schema;

namespace MovieService.Domain.Events;

[NotMapped]//prevent EF core for mapping all subclasses
public abstract class DomainEvent
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
}