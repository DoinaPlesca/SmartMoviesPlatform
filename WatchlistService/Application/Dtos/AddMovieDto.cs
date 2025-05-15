namespace WatchlistService.Application.Dtos;

public class AddMovieDto
{
    public int MovieId { get; set; }
}
//  client only sends the MovieId. All other data comes from internal MongoDB movie cache.