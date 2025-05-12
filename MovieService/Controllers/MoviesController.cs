using Microsoft.AspNetCore.Mvc;
using MovieService.Application.DTOs;
using MovieService.Application.Services;

namespace MovieService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    //  api/movies
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MovieQueryParameters query)
    {
        var movies = await _movieService.GetAllAsync(query);
        return Ok(movies);
    }

    // api/movies/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        if (movie == null) return NotFound();
        return Ok(movie);
    }

    //  api/movies
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
    {
        var created = await _movieService.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // api/movies
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateMovieDto dto)
    {
        await _movieService.UpdateAsync(dto);
        return NoContent();
    }

    //  api/movies/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _movieService.DeleteAsync(id);
        return NoContent();
    }

    //  api/movies/genre/3
    [HttpGet("genre/{genreId}")]
    public async Task<IActionResult> GetByGenre(int genreId)
    {
        var movies = await _movieService.GetByGenreAsync(genreId);
        return Ok(movies);
    }
}