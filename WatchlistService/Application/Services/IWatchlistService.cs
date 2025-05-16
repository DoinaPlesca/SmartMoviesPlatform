using WatchlistService.Application.Dtos;

namespace WatchlistService.Application.Services;

public interface IWatchlistService
{
    Task AddMovieAsync(string userId, AddMovieDto dto);
    Task RemoveMovieAsync(string userId, int movieId);
    Task<WatchlistDto> GetWatchlistAsync(string userId);
    Task RemoveMovieFromAllWatchlistsAsync(int movieId);

}
