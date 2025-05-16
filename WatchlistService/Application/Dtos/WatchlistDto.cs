namespace WatchlistService.Application.Dtos;

public class WatchlistDto
{
    public required string UserId { get; set; }
    public List<MovieItemDto> Movies { get; set; } = new();
}