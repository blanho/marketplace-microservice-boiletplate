using EventBus.Events;

namespace EventBus.Abstractions;

/// <summary>
/// Handler for a specific integration event type.
/// Register implementations in DI and the broker will dispatch to them.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

