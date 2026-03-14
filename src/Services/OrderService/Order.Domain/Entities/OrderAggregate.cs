using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.ValueObjects;
using SharedKernel;

namespace Order.Domain.Entities;

public class OrderAggregate : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public string UserName { get; private set; } = default!;
    public Address ShippingAddress { get; private set; } = default!;
    public Payment Payment { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public decimal TotalPrice => _items.Sum(i => i.UnitPrice * i.Quantity);

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    // EF Core parameterless constructor
    private OrderAggregate() { }

    public static OrderAggregate Create(
        string userName,
        Address shippingAddress,
        Payment payment,
        IEnumerable<(Guid productId, string productName, int quantity, decimal unitPrice)> items)
    {
        var order = new OrderAggregate
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            ShippingAddress = shippingAddress,
            Payment = payment,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var (productId, productName, quantity, unitPrice) in items)
        {
            order._items.Add(new OrderItem(order.Id, productId, productName, quantity, unitPrice));
        }

        order.AddDomainEvent(new OrderCreatedDomainEvent(order));
        return order;
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot process order in {Status} state.");

        Status = OrderStatus.Processing;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot complete order in {Status} state.");

        Status = OrderStatus.Completed;
        LastModifiedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderCompletedDomainEvent(Id));
    }

    public void Cancel(string reason)
    {
        if (Status is OrderStatus.Completed or OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel order in {Status} state.");

        Status = OrderStatus.Cancelled;
        LastModifiedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderCancelledDomainEvent(Id, reason));
    }
}
