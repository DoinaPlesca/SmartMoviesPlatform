using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;
using WatchlistService.Application.Interfaces;

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
        _logger.LogInformation("MovieDeletedConsumer is listening on queue 'movie-deleted'...");

        var consumer = new EventingBasicConsumer(Channel);

        consumer.Received += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var @event = JsonSerializer.Deserialize<MovieDeletedEvent>(json);

                if (@event == null || @event.Id <= 0)
                {
                    _logger.LogWarning("Received null or invalid MovieDeletedEvent.");
                    Channel.BasicAck(args.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation("Received MovieDeletedEvent for ID: {MovieId}", @event.Id);

                using var scope = ServiceProvider.CreateScope();
                var cacheRepo = scope.ServiceProvider.GetRequiredService<IMovieCacheRepository>();
                var watchlistRepo = scope.ServiceProvider.GetRequiredService<IWatchlistRepository>();

                var movieInCache = await cacheRepo.GetByIdAsync(@event.Id);
                var isZombie = movieInCache == null || string.IsNullOrWhiteSpace(movieInCache.Title);
                
                bool alreadyCleaned = await watchlistRepo.IsMovieAbsentFromAllWatchlistsAsync(@event.Id);
                if (alreadyCleaned)
                {
                    _logger.LogInformation("Movie ID {MovieId} already removed from all watchlists — skipping.", @event.Id);
                    await cacheRepo.DeleteAsync(@event.Id); 
                    Channel.BasicAck(args.DeliveryTag, multiple: false);
                    return;
                }

                if (isZombie)
                {
                    _logger.LogInformation("Movie ID {MovieId} is missing or invalid in cache — proceeding with cleanup.", @event.Id);

                    await watchlistRepo.RemoveMovieFromAllWatchlistsAsync(@event.Id);
                    await cacheRepo.DeleteAsync(@event.Id); 

                    _logger.LogInformation(" Cleaned movie {MovieId} from all watchlists and cache.", @event.Id);
                    Channel.BasicAck(args.DeliveryTag, multiple: false);
                    _logger.LogInformation("ACK sent to RabbitMQ for MovieDeletedEvent ID: {MovieId}", @event.Id);
                }
                else
                {
                    _logger.LogWarning("Movie ID {MovieId} still appears valid in cache — requeuing for retry.", @event.Id);
                    Channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process MovieDeletedEvent. Requeuing.");
                Channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
            }
        };

        Channel.BasicConsume(
            queue: "movie-deleted",
            autoAck: false,
            consumer: consumer
        );

        return Task.CompletedTask;
    }
}



