using WatchlistService.Domain.Entities;

namespace WatchlistService.Application.Interfaces;

public interface IMovieCacheRepository
{
    Task<MovieItem?> GetByIdAsync(int movieId);
    Task UpsertAsync(MovieItem movie);
    Task DeleteAsync(int movieId);
}