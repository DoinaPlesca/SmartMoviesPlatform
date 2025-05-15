using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;
using WatchlistService.Application.Interfaces;
using WatchlistService.Infrastructure.Messaging;

public class MovieUpdatedConsumer : BaseRabbitMqConsumer
{
    public MovieUpdatedConsumer(IServiceProvider serviceProvider, ILogger<MovieUpdatedConsumer> logger)
        : base(serviceProvider, logger, "movie-updated", "movies") { }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🛠️ MovieUpdatedConsumer is listening on queue 'movie-updated'...");

        var consumer = new EventingBasicConsumer(Channel);

        consumer.Received += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var @event = JsonSerializer.Deserialize<MovieUpdatedEvent>(json);

                if (@event == null)
                {
                    _logger.LogWarning("⚠️ Received null MovieUpdatedEvent.");
                    return;
                }

                using var scope = ServiceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMovieCacheRepository>();

                var existing = await repo.GetByIdAsync(@event.MovieId);
                if (existing == null)
                {
                    _logger.LogWarning("⚠️ Movie ID {MovieId} not found in cache. Cannot update.", @event.MovieId);
                    return;
                }

                // Update the fields
                existing.Title = @event.Title;
                existing.Description = @event.Description;
                existing.Genre = @event.GenreName;

                await repo.UpsertAsync(existing);

                _logger.LogInformation("✏️ Updated movie cache for Movie ID: {MovieId}", @event.MovieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to process MovieUpdatedEvent.");
            }
        };

        Channel.BasicConsume(queue: "movie-updated", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
