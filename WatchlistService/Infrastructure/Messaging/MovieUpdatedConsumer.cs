using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WatchlistService.Application.Interfaces;
using WatchlistService.Domain.Entities;

namespace WatchlistService.Infrastructure.Messaging;

public class MovieUpdatedConsumer : BaseRabbitMqConsumer
{
    public MovieUpdatedConsumer(IServiceProvider serviceProvider, ILogger<MovieUpdatedConsumer> logger)
        : base(serviceProvider, logger, "movie-updated", "movies") { }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MovieUpdatedConsumer is listening on queue 'movie-updated'...");

        var consumer = new EventingBasicConsumer(Channel);

        consumer.Received += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Id", out var idProp) || idProp.GetInt32() <= 0)
                {
                    _logger.LogWarning("Received null or invalid MovieUpdatedEvent.");
                    return;
                }

                var updatedMovie = new MovieItem
                {
                    MovieId = root.GetProperty("Id").GetInt32(),
                    Title = root.GetProperty("Title").GetString() ?? "",
                    Description = root.GetProperty("Description").GetString() ?? "",
                    Genre = root.GetProperty("GenreName").GetString() ?? "",
                    ReleaseDate = root.GetProperty("ReleaseDate").GetDateTime(),
                    Rating = root.GetProperty("Rating").GetDecimal(),
                    PosterUrl = root.GetProperty("PosterUrl").GetString() ?? "",
                    VideoUrl = root.GetProperty("VideoUrl").GetString() ?? ""
                };

                // Capture extra/unknown fields
                var knownFields = new HashSet<string>
                {
                    "Id", "Title", "Description", "GenreName", "ReleaseDate", "Rating", "PosterUrl", "VideoUrl"
                };

                var extra = new BsonDocument();

                foreach (var prop in root.EnumerateObject())
                {
                    if (!knownFields.Contains(prop.Name))
                    {
                        extra.Add(prop.Name, BsonSerializer.Deserialize<BsonValue>(prop.Value.GetRawText()));
                    }
                }

                updatedMovie.ExtraElements = extra;

                using var scope = ServiceProvider.CreateScope();
                var cacheRepo = scope.ServiceProvider.GetRequiredService<IMovieCacheRepository>();
                await cacheRepo.UpsertAsync(updatedMovie);

                _logger.LogInformation("Upserted movie cache (with extras) for Movie ID: {MovieId}", updatedMovie.MovieId);

                var watchlistRepo = scope.ServiceProvider.GetRequiredService<IWatchlistRepository>();
                await watchlistRepo.UpdateMovieInAllWatchlistsAsync(updatedMovie);

                _logger.LogInformation("Updated movie info in all watchlists for Movie ID: {MovieId}", updatedMovie.MovieId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process MovieUpdatedEvent.");
            }
        };

        Channel.BasicConsume(queue: "movie-updated", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
