using MongoDB.Driver;
using WatchlistService.Application.Interfaces;
using WatchlistService.Domain.Entities;

namespace WatchlistService.Infrastructure.Persistance.Repositories;

public class WatchlistRepository : IWatchlistRepository
{
    private readonly MongoContext _context;

    public WatchlistRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<Watchlist?> GetWatchlistByUserIdAsync(string userId)
    {
        return await _context.Watchlists.Find(w => w.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task CreateWatchlistAsync(Watchlist watchlist)
    {
        await _context.Watchlists.InsertOneAsync(watchlist);
    }

    public async Task AddMovieToWatchlistAsync(string userId, MovieItem movie)
    {
        var filter = Builders<Watchlist>.Filter.Eq(w => w.UserId, userId);
        var update = Builders<Watchlist>.Update.Push(w => w.Movies, movie);

        await _context.Watchlists.UpdateOneAsync(filter, update);
    }

    public async Task RemoveMovieFromWatchlistAsync(string userId, int movieId)
    {
        var filter = Builders<Watchlist>.Filter.Eq(w => w.UserId, userId);
        var update = Builders<Watchlist>.Update.PullFilter(w => w.Movies, m => m.MovieId == movieId);

        await _context.Watchlists.UpdateOneAsync(filter, update);
    }

}
