using MovieService.Application.DTOs;

namespace MovieService.Application.Services;

public interface IMovieService
{
    Task<IEnumerable<MovieDto>> GetAllAsync(MovieQueryParameters query);
    Task<MovieDto?> GetByIdAsync(int id);
    Task<MovieDto> CreateAsync(CreateMovieDto dto);
    Task UpdateAsync(UpdateMovieDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<GenreDto>> GetAllGenresAsync();
    Task<IEnumerable<MovieDto>> GetByGenreAsync(int genreId);
}