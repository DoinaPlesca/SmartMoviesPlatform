using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Wrappers;
using WatchlistService.Application.Dtos;
using WatchlistService.Application.Services;

namespace WatchlistService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

   
    [HttpGet("{userId}")]
    [Authorize] 
    public async Task<IActionResult> GetWatchlist(string userId)
    {
        var watchlist = await _watchlistService.GetWatchlistAsync(userId);
        return Ok(ApiResponse<WatchlistDto>.Ok(watchlist));
    }
    
    [HttpPost("{userId}")]
    [Authorize] 
    public async Task<IActionResult> AddMovie(string userId, [FromBody] AddMovieDto dto)
    {
        await _watchlistService.AddMovieAsync(userId, dto);
        return Ok(ApiResponse<string>.Ok($"Movie {dto.MovieId} added to user {userId}'s watchlist."));
    }
    
    [HttpDelete("{userId}")]
    [Authorize] 
    public async Task<IActionResult> RemoveMovie(string userId, [FromBody] RemoveMovieRequestDto dto)
    {
        await _watchlistService.RemoveMovieAsync(userId, dto.MovieId);
        return Ok(ApiResponse<string>.Ok($"Movie {dto.MovieId} removed from user {userId}'s watchlist."));
    }
}