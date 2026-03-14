using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messaging.Outbox;

/// <summary>
/// Represents an integration event persisted to the outbox table
/// within the same DB transaction as the business operation.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Payload).IsRequired();
        builder.HasIndex(x => x.ProcessedAt).HasFilter("\"ProcessedAt\" IS NULL");
    }
}
