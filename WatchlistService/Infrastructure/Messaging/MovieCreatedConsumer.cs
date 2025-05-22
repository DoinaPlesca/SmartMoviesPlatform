using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Extract known fields safely
                var movie = new MovieItem
                {
                    MovieId = root.GetProperty("Id").GetInt32(),
                    Title = root.GetProperty("Title").GetString() ?? "",
                    Description = root.GetProperty("Description").GetString() ?? "",
                    Genre = root.GetProperty("GenreName").GetString() ?? "",
                    ReleaseDate = root.GetProperty("ReleaseDate").GetDateTime(),
                    Rating = root.GetProperty("Rating").GetDecimal(),
                    PosterUrl = root.GetProperty("PosterUrl").GetString() ?? "",
                    VideoUrl = root.GetProperty("VideoUrl").GetString() ?? "",
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
                        // Convert System.Text.Json types to BsonValues
                        extra.Add(prop.Name, BsonSerializer.Deserialize<BsonValue>(prop.Value.GetRawText()));
                    }
                }

                movie.ExtraElements = extra;

                using var scope = ServiceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMovieCacheRepository>();
                await repo.UpsertAsync(movie);

                _logger.LogInformation("Cached movie (with extras): {MovieId} - {Title}", movie.MovieId, movie.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process MovieCreatedEvent.");
            }
        };

        Channel.BasicConsume(queue: "movie-created", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}
