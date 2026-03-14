namespace EventBus.Events;

/// <summary>
/// Base record for all integration events published across service boundaries.
/// Every event carries a unique Id and the UTC timestamp it was created.
/// </summary>
public record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

