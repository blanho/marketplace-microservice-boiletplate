using Messaging.Extensions;
using Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Domain.Abstractions;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureService(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── EF Core + PostgreSQL ─────────────────────────────────────────────
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null)));

        // ── Repository ───────────────────────────────────────────────────────
        services.AddScoped<IOrderRepository, OrderRepository>();

        // ── MassTransit + RabbitMQ ───────────────────────────────────────────
        services.AddMessageBroker(
            configuration,
            typeof(DependencyInjection).Assembly);

        // ── Outbox Processor ─────────────────────────────────────────────────
        services.AddHostedService<OutboxProcessor<OrderDbContext>>();

        return services;
    }
}