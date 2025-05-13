using Microsoft.EntityFrameworkCore;
using MovieService.Application.Common.Interfaces;
using MovieService.Domain.Entities;

namespace MovieService.Infrastructure.Persistence.Seeders;

public class GenreSeeder : IDataSeeder
{
    private readonly MovieDbContext _context;
    private readonly ILogger<GenreSeeder> _logger;

    public GenreSeeder(MovieDbContext context, ILogger<GenreSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var existingGenreNames = await _context.Genres
            .Select(g => g.Name)
            .ToListAsync();

        var allGenres = new[]
        {
            "Action", "Drama", "Sci-Fi", "Comedy", "Horror", "Romance", "Thriller",
            "Animation", "Family", "Mystery", "Adventure", "Fantasy", "History",
            "Music", "War", "Western", "Documentary"
        };

        var missingGenres = allGenres
            .Where(name => !existingGenreNames.Contains(name))
            .Select(name => new Genre { Name = name })
            .ToList();

        if (missingGenres.Any())
        {
            _logger.LogInformation("Seeding missing genres: {Genres}",
                string.Join(", ", missingGenres.Select(g => g.Name)));
            await _context.Genres.AddRangeAsync(missingGenres);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Missing genres seeded.");
        }
        else
        {
            _logger.LogInformation("All genres already present. No seeding needed.");
        }
    }
}