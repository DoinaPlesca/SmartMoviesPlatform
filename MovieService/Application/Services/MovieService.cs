using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieService.Application.Common.Interfaces;
using MovieService.Application.Dtos.Genre;
using MovieService.Application.Dtos.Movie;
using MovieService.Domain.Entities;
using MovieService.FeatureToogles;
using MovieService.Infrastructure.Persistence.Interfaces;
using Prometheus;
using SharedKernel.Events;
using SharedKernel.Exceptions;
using SharedKernel.Interfaces;

namespace MovieService.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repository;
    private readonly IMapper _mapper;
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<MovieService> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly IOptions<FeatureFlags> _featureFlags;
    
    private static readonly Counter FeatureFlagCounter = Metrics
        .CreateCounter("movie_feature_flag_usage_total", "Counts usage of feature flags",
            new CounterConfiguration
            {
                LabelNames = new[] { "flag_name", "status" }
            });

    private static readonly Counter MovieActionsCounter = Metrics
        .CreateCounter("movie_action_total", "Counts movie actions",
            new CounterConfiguration
            {
                LabelNames = new[] { "action" }
            });

    
    public MovieService(IMovieRepository repository, IMapper mapper, IBlobStorageService blobStorage, ILogger<MovieService> logger, IEventPublisher eventPublisher, IOptions<FeatureFlags> featureFlags)
    {
        _repository = repository;
        _mapper = mapper;
        _blobStorage = blobStorage;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _featureFlags = featureFlags;
    }

public async Task<(IEnumerable<MovieDto> Movies, int TotalCount)> GetAllAsync(MovieQueryParameters query)
{
    var moviesQuery = await _repository.GetQueryableAsync();

    LogQueryReceived(query);
    moviesQuery = ApplySearchFilter(moviesQuery, query);

    var totalCount = await moviesQuery.CountAsync();
    moviesQuery = ApplySorting(moviesQuery, query);

    var pagedMovies = await ApplyPaginationAsync(moviesQuery, query);
    LogFinalResult(totalCount, query);

    var mapped = _mapper.Map<IEnumerable<MovieDto>>(pagedMovies);
    return (mapped, totalCount);
}

private void LogQueryReceived(MovieQueryParameters query)
{
    if (_featureFlags.Value.EnableDebugLogging)
        _logger.LogDebug("Received movie query: {@Query}", query);
}

private IQueryable<Movie> ApplySearchFilter(IQueryable<Movie> queryable, MovieQueryParameters query)
{
    if (!string.IsNullOrWhiteSpace(query.Search))
    {
        var term = query.Search.ToLower();
        queryable = queryable.Where(m =>
            m.Title.ToLower().Contains(term) ||
            m.Description.ToLower().Contains(term));

        if (_featureFlags.Value.EnableDebugLogging)
            _logger.LogDebug("Applied search filter: {SearchTerm}", term);
    }
    return queryable;
}

private IQueryable<Movie> ApplySorting(IQueryable<Movie> queryable, MovieQueryParameters query)
{
    var sortBy = query.SortBy?.ToLower();

    if (sortBy == "toprated" && _featureFlags.Value.EnableTopRatedSorting)
    {
        FeatureFlagCounter.WithLabels("EnableTopRatedSorting", "enabled").Inc();
        queryable = queryable.OrderByDescending(m => m.Rating);

        if (_featureFlags.Value.EnableDebugLogging)
            _logger.LogDebug("Applied toprated sort by rating DESC (flag enabled)");
    }
    else
    {
        FeatureFlagCounter.WithLabels("EnableTopRatedSorting", "disabled").Inc();
        queryable = sortBy switch
        {
            "rating" => query.SortDescending ? queryable.OrderByDescending(m => m.Rating) : queryable.OrderBy(m => m.Rating),
            "releasedate" => query.SortDescending ? queryable.OrderByDescending(m => m.ReleaseDate) : queryable.OrderBy(m => m.ReleaseDate),
            "newest" => queryable.OrderByDescending(m => m.ReleaseDate),
            _ => query.SortDescending ? queryable.OrderByDescending(m => m.Title) : queryable.OrderBy(m => m.Title)
        };

        if (_featureFlags.Value.EnableDebugLogging)
            _logger.LogDebug("Applied fallback sort: {SortBy}, Desc: {Descending}", sortBy, query.SortDescending);
    }
    return queryable;
}

private async Task<List<Movie>> ApplyPaginationAsync(IQueryable<Movie> queryable, MovieQueryParameters query)
{
    var skip = (query.Page - 1) * query.PageSize;
    return await queryable.Skip(skip).Take(query.PageSize).ToListAsync();
}

private void LogFinalResult(int totalCount, MovieQueryParameters query)
{
    if (_featureFlags.Value.EnableDebugLogging)
    {
        _logger.LogDebug("Total movies after filtering: {Total}", totalCount);
        _logger.LogDebug("Returning page {Page} with page size {PageSize}", query.Page, query.PageSize);
    }
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
        movie.ReleaseDate = DateTime.SpecifyKind(movie.ReleaseDate, DateTimeKind.Utc);
       
        var created = await _repository.CreateAsync(movie);
        
        movie.AddCreatedEvent();
        await DomainEventDispatcher.DispatchAndClearEventsAsync(movie, _eventPublisher, "movies");
        
        _logger.LogInformation("MovieCreatedEvent published for Movie ID: {MovieId}", movie.Id);

        MovieActionsCounter.WithLabels("create").Inc();
        return _mapper.Map<MovieDto>(created);
    }
    
    public async Task<MovieDto> UpdateAsync(int id, UpdateMovieDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("Movie not found.");

        //SYNC EXEMPLE
        
        var genre = await _repository.GetGenreByIdAsync(dto.GenreId);
        if (genre == null)
            throw new BadRequestException($"Genre ID {dto.GenreId} does not exist.");
        
        //
        var posterUrl = await _blobStorage.UploadFileAsync(
            dto.PosterFile!,
            $"posters/{Guid.NewGuid()}_{dto.PosterFile.FileName}");

        var videoUrl = await _blobStorage.UploadFileAsync(
            dto.VideoFile!,
            $"videos/{Guid.NewGuid()}_{dto.VideoFile.FileName}");

        existing.PosterUrl = posterUrl;
        existing.VideoUrl = videoUrl;

        existing.UpdateRatingIfChanged(dto.Rating);

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.ReleaseDate = DateTime.SpecifyKind(dto.ReleaseDate, DateTimeKind.Utc);
        existing.GenreId = dto.GenreId;
        existing.Genre = genre;

        await _repository.UpdateAsync(existing);

        _logger.LogInformation(" Preparing MovieUpdatedEvent for ID={Id}, Title={Title}, Genre={GenreName}",
            existing.Id, existing.Title, genre?.Name ?? "null");

        existing.AddUpdatedEvent();

        foreach (var e in existing.DomainEvents)
        {
            Console.WriteLine($"Queued event: {e.GetType().Name} | ID: {((dynamic)e).Id}");
        }
//ASYNCRONOUS 

        await DomainEventDispatcher.DispatchAndClearEventsAsync(existing, _eventPublisher, "movies");
        
        existing.ClearDomainEvents();

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

        await _repository.DeleteAsync(id);
        
        movie.AddDeletedEvent();
        await DomainEventDispatcher.DispatchAndClearEventsAsync(movie, _eventPublisher, "movies");
        _logger.LogInformation("MovieDeletedEvent added for Movie ID: {MovieId}", movie.Id);
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