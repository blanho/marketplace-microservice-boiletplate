using System.Text.Json;
using EventBus.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MassTransit;

namespace Messaging.Outbox;

/// <summary>
/// Background worker that polls the OutboxMessages table for unprocessed events
/// and publishes them to RabbitMQ via MassTransit.
/// Guarantees at-least-once delivery without distributed transactions.
/// </summary>
public class OutboxProcessor<TContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor<TContext>> logger) : BackgroundService
    where TContext : DbContext
{
    private const int BatchSize = 50;
    private const int MaxRetries = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor started for {Context}", typeof(TContext).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType is null)
                {
                    logger.LogWarning("Unknown event type: {EventType}", message.EventType);
                    message.Error = $"Unknown event type: {message.EventType}";
                    message.RetryCount = MaxRetries; // Poison — stop retrying
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Payload, eventType);
                if (@event is null) continue;

                await publishEndpoint.Publish(@event, eventType, cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                logger.LogInformation(
                    "Outbox message {Id} published: {EventType}",
                    message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                logger.LogWarning(
                    ex, "Failed to publish outbox message {Id} (attempt {Retry})",
                    message.Id, message.RetryCount);
            }
        }

        if (messages.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);
    }
}
