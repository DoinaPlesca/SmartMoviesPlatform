using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WatchlistService.Domain.Entities;


public class MovieItem
{
    public int MovieId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; } 
    public required string Genre { get; set; } 
    public DateTime ReleaseDate { get; set; }
    public decimal Rating { get; set; }
    public required string PosterUrl { get; set; } 
    public required string VideoUrl { get; set; }
    
    [BsonExtraElements]
    public BsonDocument ExtraElements { get; set; } = new();
}
