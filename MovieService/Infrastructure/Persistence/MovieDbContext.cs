using Microsoft.EntityFrameworkCore;
using MovieService.Domain.Entities;

namespace MovieService.Infrastructure.Persistence;

public class MovieDbContext : DbContext
{
    public MovieDbContext(DbContextOptions<MovieDbContext> options)
        : base(options)
    {
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
}