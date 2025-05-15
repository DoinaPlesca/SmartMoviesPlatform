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
        _logger.LogInformation(" MovieDeletedConsumer is listening on queue 'movie-deleted'...");

        var consumer = new EventingBasicConsumer(Channel);

        consumer.Received += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var @event = JsonSerializer.Deserialize<MovieDeletedEvent>(json);

                if (@event == null)
                {
                    _logger.LogWarning(" Received null or malformed MovieDeletedEvent.");
                    return;
                }

                using var scope = ServiceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMovieCacheRepository>();
                await repo.DeleteAsync(@event.MovieId);

                _logger.LogInformation(" Deleted movie from cache: {MovieId}", @event.MovieId);
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