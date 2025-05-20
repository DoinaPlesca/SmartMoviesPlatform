using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieService.Application.Dtos.Genre;
using MovieService.Application.Services;
using SharedKernel.Wrappers;

namespace MovieService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly IMovieService _movieService;

    public GenresController(IMovieService movieService)
    {
        _movieService = movieService;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var genres = await _movieService.GetAllGenresAsync();
        return Ok(ApiResponse<IEnumerable<GenreDto>>.Ok(genres));
    }
}