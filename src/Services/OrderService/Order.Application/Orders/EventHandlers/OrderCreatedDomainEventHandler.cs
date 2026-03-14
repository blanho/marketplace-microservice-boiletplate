using EventBus.Events.Integration;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Events;

namespace Order.Application.Orders.EventHandlers;

/// <summary>
/// Handles the OrderCreatedDomainEvent raised when an order is created.
/// Publishes the corresponding integration event for cross-service communication.
/// In a production system with outbox pattern, this would write to the outbox table
/// instead of publishing directly.
/// </summary>
internal class OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger)
    : INotificationHandler<OrderCreatedDomainEvent>
{
    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event handled: OrderCreated — OrderId: {OrderId}, User: {UserName}",
            notification.Order.Id, notification.Order.UserName);

        // Integration event publishing is handled by the outbox processor
        // after SaveChanges commits both the order and the outbox message atomically.
        return Task.CompletedTask;
    }
}
