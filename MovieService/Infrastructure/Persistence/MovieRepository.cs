using Microsoft.EntityFrameworkCore;
using MovieService.Application.DTOs;
using MovieService.Domain.Entities;

namespace MovieService.Infrastructure.Persistence;

public class MovieRepository : IMovieRepository
{
    private readonly MovieDbContext _context;

    public MovieRepository(MovieDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(MovieQueryParameters query)
    {
        var moviesQuery = _context.Movies
            .Include(m => m.Genre)
            .AsQueryable();

        // search filter
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.ToLower();
            moviesQuery = moviesQuery.Where(m =>
                m.Title.ToLower().Contains(searchTerm) ||
                m.Description.ToLower().Contains(searchTerm));
        }

        // sorting
        moviesQuery = query.SortBy?.ToLower() switch
        {
            "rating" => query.SortDescending
                ? moviesQuery.OrderByDescending(m => m.Rating)
                : moviesQuery.OrderBy(m => m.Rating),

            "releasedate" => query.SortDescending
                ? moviesQuery.OrderByDescending(m => m.ReleaseDate)
                : moviesQuery.OrderBy(m => m.ReleaseDate),

            "newest" => moviesQuery.OrderByDescending(m => m.ReleaseDate),

            _ => query.SortDescending
                ? moviesQuery.OrderByDescending(m => m.Title)
                : moviesQuery.OrderBy(m => m.Title)
        };

        // pagination
        var skip = (query.Page - 1) * query.PageSize;

        return await moviesQuery
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync();
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