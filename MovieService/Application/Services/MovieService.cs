using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MovieService.Application.DTOs;
using MovieService.Domain.Entities;
using MovieService.Infrastructure.Persistence;

namespace MovieService.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repository;
    private readonly IMapper _mapper;
    private readonly IBlobStorageService _blobStorage;

    public MovieService(IMovieRepository repository, IMapper mapper, IBlobStorageService blobStorage)
    {
        _repository = repository;
        _mapper = mapper;
        _blobStorage = blobStorage;
    }

    public async Task<IEnumerable<MovieDto>> GetAllAsync(MovieQueryParameters query)
    {
        var moviesQuery = await _repository.GetQueryableAsync();

        // search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.ToLower();
            moviesQuery = moviesQuery.Where(m =>
                m.Title.ToLower().Contains(term) ||
                m.Description.ToLower().Contains(term));
        }

        // sort
        moviesQuery = query.SortBy?.ToLower() switch
        {
            "rating" => query.SortDescending ? moviesQuery.OrderByDescending(m => m.Rating) : moviesQuery.OrderBy(m => m.Rating),
            "releasedate" => query.SortDescending ? moviesQuery.OrderByDescending(m => m.ReleaseDate) : moviesQuery.OrderBy(m => m.ReleaseDate),
            "newest" => moviesQuery.OrderByDescending(m => m.ReleaseDate),
            _ => query.SortDescending ? moviesQuery.OrderByDescending(m => m.Title) : moviesQuery.OrderBy(m => m.Title)
        };

        // pagination
        var skip = (query.Page - 1) * query.PageSize;
        var pagedResult = await moviesQuery.Skip(skip).Take(query.PageSize).ToListAsync();

        return _mapper.Map<IEnumerable<MovieDto>>(pagedResult);
    }


    public async Task<MovieDto?> GetByIdAsync(int id)
    {
        var movie = await _repository.GetByIdAsync(id);
        return movie == null ? null : _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieDto dto)
    {
        var posterUrl = await _blobStorage.UploadFileAsync(dto.PosterFile, $"posters/{Guid.NewGuid()}_{dto.PosterFile.FileName}");
        var videoUrl = await _blobStorage.UploadFileAsync(dto.VideoFile, $"videos/{Guid.NewGuid()}_{dto.VideoFile.FileName}");
        
        var movie = _mapper.Map<Movie>(dto);
        movie.PosterUrl = posterUrl;
        movie.VideoUrl = videoUrl;
        
        var created = await _repository.CreateAsync(movie);
        return _mapper.Map<MovieDto>(created);
    }

    public async Task<MovieDto> UpdateAsync(int id, UpdateMovieDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) throw new Exception("Movie not found.");

        if (dto.PosterFile != null)
        {
            var posterUrl = await _blobStorage.UploadFileAsync(dto.PosterFile, $"posters/{Guid.NewGuid()}_{dto.PosterFile.FileName}");
            existing.PosterUrl = posterUrl;
        }

        if (dto.VideoFile != null)
        {
            var videoUrl = await _blobStorage.UploadFileAsync(dto.VideoFile, $"videos/{Guid.NewGuid()}_{dto.VideoFile.FileName}");
            existing.VideoUrl = videoUrl;
        }

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.ReleaseDate = dto.ReleaseDate;
        existing.Rating = dto.Rating;
        existing.GenreId = dto.GenreId;

        await _repository.UpdateAsync(existing);

        return _mapper.Map<MovieDto>(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var movie = await _repository.GetByIdAsync(id);
        
        await _blobStorage.DeleteFileAsync(movie.PosterUrl);
        await _blobStorage.DeleteFileAsync(movie.VideoUrl);

        await _repository.DeleteAsync(id);
        return true;
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