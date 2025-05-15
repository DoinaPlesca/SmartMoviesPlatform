using MovieService.Application.Dtos.Genre;
using MovieService.Application.Dtos.Movie;

namespace MovieService.Application.Services;

public interface IMovieService
{
    Task<(IEnumerable<MovieDto> Movies, int TotalCount)> GetAllAsync(MovieQueryParameters query);
    Task<MovieDto?> GetByIdAsync(int id);
    Task<MovieDto> CreateAsync(CreateMovieDto dto);
    Task <MovieDto>UpdateAsync( int id,UpdateMovieDto dto);
    Task <bool>DeleteAsync(int id);
    Task<IEnumerable<GenreDto>> GetAllGenresAsync();
    Task<IEnumerable<MovieDto>> GetMovieByGenreAsync(int genreId);
}