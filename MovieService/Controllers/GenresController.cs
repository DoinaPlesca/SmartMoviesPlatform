using Microsoft.AspNetCore.Mvc;
using MovieService.Application.DTOs;
using MovieService.Application.Dtos.Genre;
using MovieService.Application.DTOs.Wrappers;
using MovieService.Application.Services;

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

    //  api/genres
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var genres = await _movieService.GetAllGenresAsync();
        return Ok(ApiResponse<IEnumerable<GenreDto>>.Ok(genres));
    }
}