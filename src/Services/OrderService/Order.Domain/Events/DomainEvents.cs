using Order.Domain.Entities;
using SharedKernel;

namespace Order.Domain.Events;

public record OrderCreatedDomainEvent(OrderAggregate Order) : IDomainEvent;

public record OrderCompletedDomainEvent(Guid OrderId) : IDomainEvent;

public record OrderCancelledDomainEvent(Guid OrderId, string Reason) : IDomainEvent;
