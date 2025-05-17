using MongoDB.Driver;
using SharedKernel.Interfaces;
using WatchlistService.Application.Interfaces;
using WatchlistService.Domain.Entities;

namespace WatchlistService.Infrastructure.Persistance.Repositories;

public class MovieCacheRepository : IMovieCacheRepository
{
    //cached list of movies in MongoDB, separate from the user-specific watchlists
    private readonly MongoContext _context;

    public MovieCacheRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task<MovieItem?> GetByIdAsync(int movieId)
    {
        return await _context.Movies.Find(m => m.MovieId == movieId).FirstOrDefaultAsync();
    }

    public async Task UpsertAsync(MovieItem movie)
    {
        var filter = Builders<MovieItem>.Filter.Eq(m => m.MovieId, movie.MovieId);
        await _context.Movies.ReplaceOneAsync(filter, movie, new ReplaceOptions { IsUpsert = true });
    }

    public async Task DeleteAsync(int movieId)
    {
        await _context.Movies.DeleteOneAsync(m => m.MovieId == movieId);
    }
}