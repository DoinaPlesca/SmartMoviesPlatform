using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MovieService.Application.Common.Interfaces;
using MovieService.Application.Dtos.Genre;
using MovieService.Application.Dtos.Movie;
using MovieService.Domain.Entities;
using MovieService.Infrastructure.Persistence.Interfaces;
using SharedKernel.Exceptions;

namespace MovieService.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repository;
    private readonly IMapper _mapper;
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<MovieService> _logger;

    public MovieService(IMovieRepository repository, IMapper mapper, IBlobStorageService blobStorage, ILogger<MovieService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _blobStorage = blobStorage;
        _logger = logger;
    }

    public async Task<(IEnumerable<MovieDto> Movies, int TotalCount)> GetAllAsync(MovieQueryParameters query)
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
        
        var totalCount = await moviesQuery.CountAsync();

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
        var pagedMovies = await moviesQuery.Skip(skip).Take(query.PageSize).ToListAsync();

        var mapped = _mapper.Map<IEnumerable<MovieDto>>(pagedMovies);
        return (mapped, totalCount);
    }


    public async Task<MovieDto?> GetByIdAsync(int id)
    {
        var movie = await _repository.GetByIdAsync(id);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {id} not found.");
        return _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieDto dto)
    {
        var posterUrl = await _blobStorage.UploadFileAsync(dto.PosterFile, $"posters/{Guid.NewGuid()}_{dto.PosterFile.FileName}");
        var videoUrl = await _blobStorage.UploadFileAsync(dto.VideoFile, $"videos/{Guid.NewGuid()}_{dto.VideoFile.FileName}");
        
        var movie = _mapper.Map<Movie>(dto);
        movie.PosterUrl = posterUrl;
        movie.VideoUrl = videoUrl;
        
        var genre = await _repository.GetGenreByIdAsync(dto.GenreId);
        if (genre == null)
            throw new BadRequestException($"Genre ID {dto.GenreId} does not exist.");
        
        movie.Genre = genre;
        
        movie.AddCreatedEvent();
        _logger.LogInformation("MovieCreatedEvent added for Movie ID: {MovieId}", movie.Id);
        
        var created = await _repository.CreateAsync(movie);
        return _mapper.Map<MovieDto>(created);
    }

    public async Task<MovieDto> UpdateAsync(int id, UpdateMovieDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) throw new NotFoundException("Movie not found.");

        var genre = await _repository.GetGenreByIdAsync(dto.GenreId);
        if (genre == null)
            throw new BadRequestException($"Genre ID {dto.GenreId} does not exist.");

        if (dto.PosterFile != null)
        {
            var posterUrl = await _blobStorage.UploadFileAsync(dto.PosterFile, $"posters/{Guid.NewGuid()}_{dto.PosterFile.FileName}");
            existing.PosterUrl = posterUrl;
            existing.AddPosterReplacedEvent();
            _logger.LogInformation("PosterReplacedEvent added for Movie ID: {MovieId}", existing.Id);
        }

        if (dto.VideoFile != null)
        {
            var videoUrl = await _blobStorage.UploadFileAsync(dto.VideoFile, $"videos/{Guid.NewGuid()}_{dto.VideoFile.FileName}");
            existing.VideoUrl = videoUrl;
            existing.AddVideoFileUpdatedEvent();
            _logger.LogInformation("VideoFileUpdatedEvent added for Movie ID: {MovieId}", existing.Id);
        }
        existing.UpdateRatingIfChanged(dto.Rating);
        _logger.LogInformation("MovieRatedEvent added for Movie ID: {MovieId} if rating changed", existing.Id);
        
        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.ReleaseDate = dto.ReleaseDate;
        existing.GenreId = dto.GenreId;

        existing.AddUpdatedEvent();
        await _repository.UpdateAsync(existing);
        _logger.LogInformation("MovieUpdatedEvent added for Movie ID: {MovieId}", existing.Id);
        return _mapper.Map<MovieDto>(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var movie = await _repository.GetByIdAsync(id);
        
        if (movie == null)
            throw new NotFoundException("Movie not found.");
        
        await _blobStorage.DeleteFileAsync(movie.PosterUrl);
        await _blobStorage.DeleteFileAsync(movie.VideoUrl);

        movie.AddDeletedEvent();
        _logger.LogInformation("MovieDeletedEvent added for Movie ID: {MovieId}", movie.Id);
        
        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<GenreDto>> GetAllGenresAsync()
    {
        var genres = await _repository.GetAllGenresAsync();
        return _mapper.Map<IEnumerable<GenreDto>>(genres);
    }

    
    public async Task<IEnumerable<MovieDto>> GetMovieByGenreAsync(int genreId)
    {
        var genre = await _repository.GetGenreByIdAsync(genreId);
        if (genre == null)
            throw new NotFoundException($"Genre with ID {genreId} not found.");

        var movies = await _repository.GetByGenreAsync(genreId);
        return _mapper.Map<IEnumerable<MovieDto>>(movies);
    }
}