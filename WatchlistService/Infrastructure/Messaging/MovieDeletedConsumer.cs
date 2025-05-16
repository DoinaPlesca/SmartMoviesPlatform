
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;
using WatchlistService.Application.Interfaces;
using WatchlistService.Application.Services;

namespace WatchlistService.Infrastructure.Messaging;

public class MovieDeletedConsumer : BaseRabbitMqConsumer
{
    private readonly ILogger<MovieDeletedConsumer> _logger;

    public MovieDeletedConsumer(IServiceProvider serviceProvider, ILogger<MovieDeletedConsumer> logger)
        : base(serviceProvider, logger, "movie-deleted", "movies")
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(" MovieDeletedConsumer is listening on queue 'movie-deleted'...");

        var consumer = new EventingBasicConsumer(Channel);

        consumer.Received += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var @event = JsonSerializer.Deserialize<MovieDeletedEvent>(json);

                if (@event == null || @event.Id <= 0)
                {
                    _logger.LogWarning("⚠️ Received null or invalid MovieDeletedEvent.");
                    return;
                }

// Optional: log all received deletes for traceability
                _logger.LogInformation("📥 Received MovieDeletedEvent for ID: {MovieId}", @event.Id);

// Double-check cache before deleting from watchlist (optional safety layer)
                using var scope = ServiceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMovieCacheRepository>();

// 👇 Check if movie is still in cache — if yes, skip deletion
                var movieInCache = await repo.GetByIdAsync(@event.Id);
                if (movieInCache != null)
                {
                    _logger.LogWarning("⚠️ Movie ID {MovieId} still in cache — skipping delete to prevent conflict.", @event.Id);
                    return;
                }

// Proceed with deletion if confirmed removed from cache
                var watchlistRepo = scope.ServiceProvider.GetRequiredService<IWatchlistRepository>();
                await watchlistRepo.RemoveMovieFromAllWatchlistsAsync(@event.Id);
                await repo.DeleteAsync(@event.Id);

                _logger.LogInformation("🗑️ Deleted movie {MovieId} from all watchlists and cache.", @event.Id);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to process MovieDeletedEvent.");
            }
        };


        Channel.BasicConsume(
            queue: "movie-deleted",
            autoAck: true,
            consumer: consumer
        );

        return Task.CompletedTask;
    }
}