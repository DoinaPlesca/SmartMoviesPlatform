namespace WatchlistService.Application.Dtos;

public class WatchlistDto
{
    public string UserId { get; set; } = null!;
    public List<MovieItemDto> Movies { get; set; } = new();
}