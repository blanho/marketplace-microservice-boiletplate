using System.Text.Json;
using EventBus.Events;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Outbox;

/// <summary>
/// Extension methods for writing integration events to the outbox table
/// within the same transaction as business data changes.
/// </summary>
public static class OutboxExtensions
{
    /// <summary>
    /// Adds an integration event to the OutboxMessages table.
    /// Call SaveChangesAsync after this to persist both business data and the outbox entry atomically.
    /// </summary>
    public static void AddOutboxMessage<TEvent>(this DbContext context, TEvent @event)
        where TEvent : IntegrationEvent
    {
        var message = new OutboxMessage
        {
            Id = @event.Id,
            EventType = typeof(TEvent).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(@event),
            CreatedAt = @event.CreatedAt
        };

        context.Set<OutboxMessage>().Add(message);
    }
}
