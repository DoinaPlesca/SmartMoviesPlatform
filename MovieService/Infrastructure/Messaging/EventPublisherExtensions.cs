using MovieService.Application.Common.Interfaces;
using MovieService.Domain.Events;

namespace MovieService.Infrastructure.Messaging;

public static class EventPublisherExtensions
{
    public static Task PublishDynamicAsync(this IEventPublisher publisher, DomainEvent e, string topic)
    {
        var method = typeof(IEventPublisher).GetMethod(nameof(IEventPublisher.PublishAsync))!;
        var generic = method.MakeGenericMethod(e.GetType());
        return (Task)generic.Invoke(publisher, new object[] { topic, e })!;
    }
}