using WatchlistService.Domain.Entities;

namespace WatchlistService.Application.Interfaces;

public interface IWatchlistRepository
{
    Task<Watchlist?> GetWatchlistByUserIdAsync(string userId);
    Task CreateWatchlistAsync(Watchlist watchlist);
    Task AddMovieToWatchlistAsync(string userId, MovieItem movie);
    Task RemoveMovieFromWatchlistAsync(string userId, int movieId);

}
