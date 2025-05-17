using MongoDB.Bson;
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

    //removes a movie from one user's list ,when a user manually removes a movie
    public async Task RemoveMovieFromWatchlistAsync(string userId, int movieId)
    {
        var filter = Builders<Watchlist>.Filter.Eq(w => w.UserId, userId);
        var update = Builders<Watchlist>.Update.PullFilter(w => w.Movies, m => m.MovieId == movieId);

        await _context.Watchlists.UpdateOneAsync(filter, update);
    }
    
    
    //when a movie is deleted globally via MovieDeletedEvent, removes the movie from all users
    public async Task RemoveMovieFromAllWatchlistsAsync(int movieId)
    {
        var filter = Builders<Watchlist>.Filter.ElemMatch(w => w.Movies, m => m.MovieId == movieId);
        var update = Builders<Watchlist>.Update.PullFilter(w => w.Movies, m => m.MovieId == movieId);

        await _context.Watchlists.UpdateManyAsync(filter, update);
    }
    
    
    //updates all fields of a movie in all watchlists when a movie is updated (via MovieUpdatedEvent)
    public async Task UpdateMovieInAllWatchlistsAsync(MovieItem movie)
    {
        var filter = Builders<Watchlist>.Filter.ElemMatch(w => w.Movies, m => m.MovieId == movie.MovieId);

        var update = Builders<Watchlist>.Update
            .Set("Movies.$[elem].Title", movie.Title)
            .Set("Movies.$[elem].Description", movie.Description)
            .Set("Movies.$[elem].Genre", movie.Genre)
            .Set("Movies.$[elem].ReleaseDate", movie.ReleaseDate)
            .Set("Movies.$[elem].Rating", movie.Rating)
            .Set("Movies.$[elem].PosterUrl", movie.PosterUrl)
            .Set("Movies.$[elem].VideoUrl", movie.VideoUrl);

        var options = new UpdateOptions
        {
            ArrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("elem.MovieId", movie.MovieId)
                )
            }
        };

        await _context.Watchlists.UpdateManyAsync(filter, update, options);
    }
    
    
    public async Task<bool> IsMovieAbsentFromAllWatchlistsAsync(int movieId)
    {
        var filter = Builders<Watchlist>.Filter.ElemMatch(w => w.Movies, m => m.MovieId == movieId);
        var existing = await _context.Watchlists.Find(filter).AnyAsync();
        return !existing;
    }

}
