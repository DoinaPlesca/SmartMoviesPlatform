namespace MovieService.Application.Common.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T message);
}