using MovieService.Domain.Entities;

namespace MovieService.Infrastructure.Persistence;

public interface IMovieRepository
{
    Task<IQueryable<Movie>> GetQueryableAsync();
    Task<Movie?> GetByIdAsync(int id);
    Task<Movie> CreateAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task DeleteAsync(int id);
    Task<bool> SaveChangesAsync();
    
    Task<IEnumerable<Genre>> GetAllGenresAsync();
    Task<Genre?> GetGenreByIdAsync(int id);
    Task<IEnumerable<Movie>> GetByGenreAsync(int genreId);

}
 
