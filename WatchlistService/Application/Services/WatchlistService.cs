using AutoMapper;
using SharedKernel.Exceptions;
using WatchlistService.Application.Dtos;
using WatchlistService.Application.Interfaces;
using WatchlistService.Domain.Entities;

namespace WatchlistService.Application.Services;

public class WatchlistService : IWatchlistService
{
    private readonly IWatchlistRepository _repository;
    private readonly IMovieCacheRepository _movieCache;
    private readonly IMapper _mapper;
    private readonly ILogger<WatchlistService> _logger;

    public WatchlistService(
        IWatchlistRepository repository,
        IMovieCacheRepository movieCache,
        IMapper mapper,
        ILogger<WatchlistService> logger)
    {
        _repository = repository;
        _movieCache = movieCache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WatchlistDto> GetWatchlistAsync(string userId)
    {
        var watchlist = await _repository.GetWatchlistByUserIdAsync(userId)
            ?? throw new NotFoundException($"Watchlist for user {userId} not found.");

        return _mapper.Map<WatchlistDto>(watchlist);
    }

    public async Task AddMovieAsync(string userId, AddMovieDto dto)
    {
        MovieItem? movie = null;
        const int retryCount = 3;

        for (int i = 0; i < retryCount; i++)
        {
            movie = await _movieCache.GetByIdAsync(dto.MovieId);
            if (movie != null)
                break;

            await Task.Delay(500); 
        }

        if (movie == null)
            throw new NotFoundException($"Movie with ID {dto.MovieId} not found in local cache.");

        var watchlist = await _repository.GetWatchlistByUserIdAsync(userId);

        if (watchlist == null)
        {
            watchlist = new Watchlist
            {
                UserId = userId,
                Movies = new List<MovieItem> { movie }
            };

            await _repository.CreateWatchlistAsync(watchlist);
        }
        else
        {
            if (watchlist.Movies.Any(m => m.MovieId == dto.MovieId))
                throw new BadRequestException($"Movie ID {dto.MovieId} is already in the watchlist.");

            await _repository.AddMovieToWatchlistAsync(userId, movie);
        }

        _logger.LogInformation("Movie {MovieId} added to user {UserId}'s watchlist", dto.MovieId, userId);
    }


    public async Task RemoveMovieAsync(string userId, int movieId)
    {
        var watchlist = await _repository.GetWatchlistByUserIdAsync(userId);
        if (watchlist == null)
            throw new NotFoundException($"Watchlist for user {userId} not found.");

        var exists = watchlist.Movies.Any(m => m.MovieId == movieId);
        if (!exists)
            throw new NotFoundException($"Movie ID {movieId} not found in user's watchlist.");

        await _repository.RemoveMovieFromWatchlistAsync(userId, movieId);
        _logger.LogInformation("Movie {MovieId} removed from user {UserId}'s watchlist", movieId, userId);
    }
    
    public async Task RemoveMovieFromAllWatchlistsAsync(int movieId)
    {
        await _repository.RemoveMovieFromAllWatchlistsAsync(movieId);
    }
}
