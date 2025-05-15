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

public class MovieItem
{
    public int MovieId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public DateTime ReleaseDate { get; set; }
    public decimal Rating { get; set; }
    public string PosterUrl { get; set; } = null!;
    public string VideoUrl { get; set; } = null!;
}

