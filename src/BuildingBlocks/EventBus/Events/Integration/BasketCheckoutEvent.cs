using EventBus.Events;

namespace EventBus.Events.Integration;

/// <summary>
/// Published by BasketService when a user checks out.
/// Consumed by OrderService to create a new order.
/// </summary>
public record BasketCheckoutEvent : IntegrationEvent
{
    // Identity
    public string UserName { get; init; } = default!;

    // Basket snapshot
    public decimal TotalPrice { get; init; }
    public List<BasketCheckoutItem> Items { get; init; } = [];

    // Shipping
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string EmailAddress { get; init; } = default!;
    public string AddressLine { get; init; } = default!;
    public string Country { get; init; } = default!;
    public string State { get; init; } = default!;
    public string ZipCode { get; init; } = default!;

    // Payment
    public string CardName { get; init; } = default!;
    public string CardNumber { get; init; } = default!;
    public string Expiration { get; init; } = default!;
    public string Cvv { get; init; } = default!;
    public int PaymentMethod { get; init; }
}

public record BasketCheckoutItem
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}
