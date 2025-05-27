using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WatchlistService.Domain.Entities;

/// <summary>
/// Represents a user's movie watchlist.
///
/// Informal specification:
/// - Each watchlist is linked to a specific user.
/// - Stores a list of movies added by the user.
/// </summary>
public class Watchlist
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!; 
    public List<MovieItem> Movies { get; set; } = new();
}

