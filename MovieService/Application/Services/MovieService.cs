using AutoMapper;
using MovieService.Application.DTOs;
using MovieService.Domain.Entities;
using MovieService.Infrastructure.Persistence;

namespace MovieService.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repository;
    private readonly IMapper _mapper;

    public MovieService(IMovieRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MovieDto>> GetAllAsync(MovieQueryParameters query)
    {
        var movies = await _repository.GetAllAsync(query);
        return _mapper.Map<IEnumerable<MovieDto>>(movies);
    }

    public async Task<MovieDto?> GetByIdAsync(int id)
    {
        var movie = await _repository.GetByIdAsync(id);
        return movie == null ? null : _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieDto dto)
    {
        var movie = _mapper.Map<Movie>(dto);
        var created = await _repository.CreateAsync(movie);
        return _mapper.Map<MovieDto>(created);
    }

    public async Task UpdateAsync(UpdateMovieDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.Id);
        if (existing == null) throw new Exception("Movie not found.");

        _mapper.Map(dto, existing);
        await _repository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<GenreDto>> GetAllGenresAsync()
    {
        var genres = await _repository.GetAllGenresAsync();
        return _mapper.Map<IEnumerable<GenreDto>>(genres);
    }

    public async Task<IEnumerable<MovieDto>> GetByGenreAsync(int genreId)
    {
        var movies = await _repository.GetByGenreAsync(genreId);
        return _mapper.Map<IEnumerable<MovieDto>>(movies);
    }
}