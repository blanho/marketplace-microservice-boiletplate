using EventBus.Events;

namespace EventBus.Events.Integration;

/// <summary>
/// Published by OrderService when an order is successfully created.
/// Can be consumed by notification services, analytics, etc.
/// </summary>
public record OrderCreatedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string UserName { get; init; } = default!;
    public decimal TotalPrice { get; init; }
    public List<OrderCreatedItem> Items { get; init; } = [];
}

public record OrderCreatedItem
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}
