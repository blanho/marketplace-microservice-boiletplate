using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Extensions;

public static class MessagingExtensions
{
    /// <summary>
    /// Registers MassTransit with RabbitMQ transport.
    /// Discovers all consumers in the specified assemblies.
    /// </summary>
    public static IServiceCollection AddMessageBroker(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] consumerAssemblies)
    {
        services.AddMassTransit(busConfig =>
        {
            busConfig.SetKebabCaseEndpointNameFormatter();

            // Register all IConsumer<T> implementations from provided assemblies
            if (consumerAssemblies.Length > 0)
                busConfig.AddConsumers(consumerAssemblies);

            busConfig.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = configuration.GetConnectionString("MessageBroker")
                    ?? "amqp://guest:guest@localhost:5672";

                cfg.Host(new Uri(connectionString));

                // Retry policy: exponential backoff for transient consumer failures
                cfg.UseMessageRetry(retryConfig =>
                {
                    retryConfig.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromMilliseconds(200),
                        maxInterval: TimeSpan.FromSeconds(5),
                        intervalDelta: TimeSpan.FromMilliseconds(500));
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
