using EventBus.Events;

namespace EventBus.Abstractions;

/// <summary>
/// Abstraction for publishing and subscribing to integration events.
/// Implement this in the Messaging project (e.g. RabbitMQ, Azure Service Bus).
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : IntegrationEvent;
}

