using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;
using WatchlistService.Application.Interfaces;
using WatchlistService.Domain.Entities;

namespace WatchlistService.Infrastructure.Messaging;

public class MovieCreatedConsumer : BaseRabbitMqConsumer
{
    private readonly ILogger<MovieCreatedConsumer> _logger;

    public MovieCreatedConsumer(IServiceProvider serviceProvider, ILogger<MovieCreatedConsumer> logger)
        : base(serviceProvider, logger, "movie-created", "movies")
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MovieCreatedConsumer is listening on queue 'movie-created'...");

        var consumer = new EventingBasicConsumer(Channel);
        consumer.Received += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var @event = JsonSerializer.Deserialize<MovieCreatedEvent>(json);

                if (@event == null)
                {
                    _logger.LogWarning(" Received null or malformed MovieCreatedEvent.");
                    return;
                }

                var movie = new MovieItem
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

                using var scope = ServiceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMovieCacheRepository>();
                await repo.UpsertAsync(movie);

                _logger.LogInformation("Cached movie: {MovieId} - {Title}", movie.MovieId, movie.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to process MovieCreatedEvent.");
            }
        };

        Channel.BasicConsume(
            queue: "movie-created",
            autoAck: true,
            consumer: consumer
        );

        return Task.CompletedTask;
    }
}
