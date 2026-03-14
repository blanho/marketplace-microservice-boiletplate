using MediatR;

namespace SharedKernel;

/// <summary>
/// Marker interface for domain events raised within an aggregate.
/// Extends INotification so MediatR can dispatch domain events.
/// </summary>
public interface IDomainEvent : INotification;

