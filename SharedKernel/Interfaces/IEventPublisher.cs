namespace SharedKernel.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T message);
}