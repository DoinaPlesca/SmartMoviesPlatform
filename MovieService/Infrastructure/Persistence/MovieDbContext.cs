using Microsoft.EntityFrameworkCore;
using MovieService.Application.Common.Interfaces;
using MovieService.Domain.Entities;
using MovieService.Domain.Events;
using MovieService.Infrastructure.Messaging;

namespace MovieService.Infrastructure.Persistence;

public class MovieDbContext : DbContext
{
    private readonly IEventPublisher _eventPublisher;
    public MovieDbContext(DbContextOptions<MovieDbContext> options, IEventPublisher eventPublisher)
        : base(options)
    {
        _eventPublisher = eventPublisher;
    }

    public DbSet<Movie> Movies { get; set; }

    public DbSet<Genre> Genres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Genre>()
            .HasMany(g => g.Movies)
            .WithOne(m => m.Genre)
            .HasForeignKey(m => m.GenreId);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        var domainEntities = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var events = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in events)
        {
            await _eventPublisher.PublishDynamicAsync(domainEvent, "movies");
        }
        return result;
    }
}
