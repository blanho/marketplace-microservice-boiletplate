using MediatR;
using Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using SharedKernel;

namespace Order.Infrastructure.Data;

public class OrderDbContext(
    DbContextOptions<OrderDbContext> options,
    IMediator mediator) : DbContext(options)
{
    public DbSet<OrderAggregate> Orders => Set<OrderAggregate>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // OutboxMessage configuration
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // Order aggregate
        modelBuilder.Entity<OrderAggregate>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);

            entity.Property(o => o.UserName).IsRequired().HasMaxLength(100);
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");

            entity.OwnsOne(o => o.ShippingAddress, address =>
            {
                address.WithOwner();
                address.Property(a => a.FirstName).HasMaxLength(50).IsRequired();
                address.Property(a => a.LastName).HasMaxLength(50).IsRequired();
                address.Property(a => a.EmailAddress).HasMaxLength(100);
                address.Property(a => a.AddressLine).HasMaxLength(200);
                address.Property(a => a.Country).HasMaxLength(50);
                address.Property(a => a.State).HasMaxLength(50);
                address.Property(a => a.ZipCode).HasMaxLength(20);
            });

            entity.OwnsOne(o => o.Payment, payment =>
            {
                payment.WithOwner();
                payment.Property(p => p.CardName).HasMaxLength(50);
                payment.Property(p => p.CardNumber).HasMaxLength(30);
                payment.Property(p => p.Expiration).HasMaxLength(10);
                payment.Property(p => p.Cvv).HasMaxLength(5);
            });

            entity.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(o => o.DomainEvents);
        });

        // Order items
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }

    /// <summary>
    /// Dispatches domain events after saving changes.
    /// Ensures events are only published after the transaction succeeds.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events before saving (entities may be detached after save)
        var aggregates = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Save changes (business data + outbox messages in same transaction)
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }

        // Clear events to prevent re-publishing
        aggregates.ForEach(a => a.ClearDomainEvents());

        return result;
    }
}
