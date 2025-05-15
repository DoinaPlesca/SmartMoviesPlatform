using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WatchlistService.Domain.Entities;

public class Watchlist
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public List<MovieItem> Movies { get; set; } = new();
}
