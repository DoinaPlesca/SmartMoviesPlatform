using RabbitMQ.Client;


namespace WatchlistService.Infrastructure.Messaging;

public abstract class BaseRabbitMqConsumer : BackgroundService
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger<BaseRabbitMqConsumer> _logger;
    protected IModel Channel;
    private IConnection _connection;

    protected BaseRabbitMqConsumer(IServiceProvider serviceProvider, ILogger<BaseRabbitMqConsumer> logger, string queueName, string exchangeName)
    {
        ServiceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq"
        };

        _connection = factory.CreateConnection();
        Channel = _connection.CreateModel();

        // Declare durable exchange and queue
        Channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);
        Channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
        Channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");

        _logger.LogInformation("Initialized RabbitMQ consumer: Exchange='{Exchange}', Queue='{Queue}'", exchangeName, queueName);
    }

    public override void Dispose()
    {
        Channel?.Close();
        Channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}