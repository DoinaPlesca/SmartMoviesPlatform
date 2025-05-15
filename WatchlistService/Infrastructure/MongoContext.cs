using MongoDB.Driver;
using WatchlistService.Domain.Entities;

namespace WatchlistService.Infrastructure;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
                               ?? throw new ArgumentNullException("MONGO_CONNECTION_STRING is missing");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("WatchlistDb");
    }

    public IMongoCollection<Watchlist> Watchlists => _database.GetCollection<Watchlist>("Watchlists");
    
    //movies collection for local movie cache
    public IMongoCollection<MovieItem> Movies => _database.GetCollection<MovieItem>("Movies");
}