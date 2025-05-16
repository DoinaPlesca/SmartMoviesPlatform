using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;
using WatchlistService.Application.Interfaces;
using WatchlistService.Domain.Entities;
using WatchlistService.Infrastructure.Messaging;

public class MovieUpdatedConsumer : BaseRabbitMqConsumer
{
    public MovieUpdatedConsumer(IServiceProvider serviceProvider, ILogger<MovieUpdatedConsumer> logger)
        : base(serviceProvider, logger, "movie-updated", "movies") { }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(" MovieUpdatedConsumer is listening on queue 'movie-updated'...");

        var consumer = new EventingBasicConsumer(Channel);

        consumer.Received += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var @event = JsonSerializer.Deserialize<MovieUpdatedEvent>(json);

                if (@event == null || @event.Id <= 0)
                {
                    _logger.LogWarning("Received null or invalid MovieUpdatedEvent.");
                    return;
                }

                using var scope = ServiceProvider.CreateScope();
                
                var cacheRepo = scope.ServiceProvider.GetRequiredService<IMovieCacheRepository>();

                var updatedMovie = new MovieItem
                {
                    MovieId = @event.Id,
                    Title = @event.Title,
                    Description = @event.Description,
                    Genre = @event.GenreName,
                    ReleaseDate = @event.ReleaseDate,
                    Rating = @event.Rating,
                    PosterUrl = @event.PosterUrl,
                    VideoUrl = @event.VideoUrl
                };

                await cacheRepo.UpsertAsync(updatedMovie);
                _logger.LogInformation(" Upserted movie cache for Movie ID: {MovieId}", @event.Id);
                
                var watchlistRepo = scope.ServiceProvider.GetRequiredService<IWatchlistRepository>();
                await watchlistRepo.UpdateMovieInAllWatchlistsAsync(updatedMovie);

                _logger.LogInformation(" Updated movie info in all watchlists for Movie ID: {MovieId}", @event.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to process MovieUpdatedEvent.");
            }
        };


        Channel.BasicConsume(queue: "movie-updated", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
