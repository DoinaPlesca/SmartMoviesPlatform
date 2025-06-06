using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieService.Application.Dtos.Movie;
using MovieService.Application.Services;
using SharedKernel.Wrappers;

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

   
    [HttpGet]
    [Authorize]
    // [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] MovieQueryParameters query)
    {
        var (movies, totalCount) = await _movieService.GetAllAsync(query);

        return Ok(PagedResponse<IEnumerable<MovieDto>>.Ok(
            movies,
            totalCount,
            query.Page,
            query.PageSize
        ));
    }
    
 
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        if (movie == null)
            return NotFound(ApiResponse<string>.Fail("Movie not found", 404));
        
        return Ok(ApiResponse<MovieDto>.Ok(movie));
    }

    
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMovie([FromForm] CreateMovieDto dto)
    {
        var created = await _movieService.CreateAsync(dto);
        return Ok(ApiResponse<MovieDto>.Ok(created, 201));
    }

   
    [HttpPut("{id}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateMovieDto dto)
    {
        var updated= await _movieService.UpdateAsync(id, dto);
        return Ok(ApiResponse<MovieDto>.Ok(updated));
    }


    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _movieService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<string>.Fail("Movie not found", 404));

        return Ok(ApiResponse<string>.Ok("Movie deleted successfully", 200));
    }
    
    [HttpGet("genre/{genreId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByGenre(int genreId)
    {
        var movies = await _movieService.GetMovieByGenreAsync(genreId);
        return Ok(ApiResponse<IEnumerable<MovieDto>>.Ok(movies));
    }
}