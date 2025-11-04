using SharedKernel.Events;
using SharedKernel.Interfaces;

// Dynamically publishes a domain event using the IEventPublisher interface,
// resolving the correct generic method at runtime based on the actual event type.
// uses reflection to call the correct PublishAsync<T> method with the right generic type at runtime.
namespace SharedKernel.Extensions;

public static class EventPublisherExtensions
{
    public static Task PublishDynamicAsync(this IEventPublisher publisher, DomainEvent e, string topic)
    {
        var method = typeof(IEventPublisher).GetMethod(nameof(IEventPublisher.PublishAsync))!;
        var generic = method.MakeGenericMethod(e.GetType());
        return (Task)generic.Invoke(publisher, new object[] { topic, e })!;
    }
}