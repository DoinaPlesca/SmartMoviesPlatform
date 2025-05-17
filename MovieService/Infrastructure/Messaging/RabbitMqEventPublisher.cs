using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SharedKernel.Interfaces;
using IModel = RabbitMQ.Client.IModel;

namespace MovieService.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly string _exchangeName;
    private readonly string _exchangeType;

    public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;

        var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST")
                       ?? throw new InvalidOperationException("RABBITMQ_HOST environment variable is missing");

        _exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE_NAME") ?? "movies";
        _exchangeType = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE_TYPE") ?? ExchangeType.Direct;

        var factory = new ConnectionFactory() { HostName = hostName };

        const int maxRetries = 5;
        for (int i = 1; i <= maxRetries; i++)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                _channel.ExchangeDeclare(exchange: _exchangeName, type: _exchangeType, durable: true);

                _logger.LogInformation("Connected to RabbitMQ and declared exchange '{Exchange}'", _exchangeName);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ connection failed (attempt {Attempt}/{Max})", i, maxRetries);
                if (i == maxRetries)
                    throw;
                Thread.Sleep(3000);
            }
        }
    }

    public async Task PublishAsync<T>(string topic, T message)
    {
        await PublishWithRetryAsync(topic, message);
    }

    
    private async Task PublishWithRetryAsync<T>(string topic, T message)
    {
        const int retries = 3;

        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                
                _channel.BasicPublish(
                    exchange: topic,
                    routingKey: "",
                    basicProperties: null,
                    body: body);

                _logger.LogInformation("Published event {EventType} to topic '{Topic}'", typeof(T).Name, topic);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Publish attempt {Attempt} failed for {EventType}", attempt, typeof(T).Name);

                if (attempt == retries)
                {
                    _logger.LogError(ex, "Failed to publish {EventType} after {Retries} attempts", typeof(T).Name, retries);
                    throw;
                }

                await Task.Delay(1000);
            }
        }
    }


    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}