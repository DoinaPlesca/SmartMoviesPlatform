using Microsoft.EntityFrameworkCore;
using MovieService.Domain.Entities;
using MovieService.Infrastructure.Persistence.Interfaces;

namespace MovieService.Infrastructure.Persistence.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MovieDbContext _context;

    public MovieRepository(MovieDbContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<Movie>> GetQueryableAsync()
    {
        return await Task.FromResult(
            _context.Movies
                .Include(m => m.Genre)
                .AsNoTracking()
        );
    }

    public async Task<Movie?> GetByIdAsync(int id)
    {
        return await _context.Movies
            .Include(m => m.Genre)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Movie> CreateAsync(Movie movie)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return movie;
    }
    public async Task UpdateAsync(Movie movie)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id )
    {
        var movie = await GetByIdAsync(id);
        if (movie != null)
        {
           _context.Movies.Remove(movie);
           await _context.SaveChangesAsync();
        }
    }
    public async Task<IEnumerable<Genre>> GetAllGenresAsync()
    {
        return await _context.Genres.ToListAsync();
    }

    public async Task<Genre?> GetGenreByIdAsync(int id)
    {
        return await _context.Genres.FirstOrDefaultAsync(g => g.Id == id);
    }
    
    public async Task<IEnumerable<Movie>> GetByGenreAsync(int genreId)
    {
        return await _context.Movies
            .Where(m => m.GenreId == genreId)
            .Include(m => m.Genre)
            .ToListAsync();
    }


    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
    
    // TODO: Raise MovieDeletedEvent and notify WatchlistService
}