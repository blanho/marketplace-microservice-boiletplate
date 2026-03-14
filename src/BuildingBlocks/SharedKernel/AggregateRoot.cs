namespace SharedKernel;

/// <summary>
/// Base class for aggregate roots. Extends Entity with a domain events collection.
/// Raise events via AddDomainEvent(); dispatch them in the infrastructure layer before saving.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

