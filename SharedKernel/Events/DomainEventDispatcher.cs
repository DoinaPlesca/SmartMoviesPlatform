using SharedKernel.Extensions;
using SharedKernel.Interfaces;

namespace SharedKernel.Events;

public static class DomainEventDispatcher
{
    public static async Task DispatchAndClearEventsAsync(Entity entity, IEventPublisher publisher, string topic)
    {
        foreach (var domainEvent in entity.DomainEvents)
        {
            Console.WriteLine($" Dispatching: {domainEvent.GetType().Name} for Movie ID: {((dynamic)domainEvent).Id}");
            await publisher.PublishDynamicAsync(domainEvent, topic);
        }

        entity.ClearDomainEvents();
    }
}