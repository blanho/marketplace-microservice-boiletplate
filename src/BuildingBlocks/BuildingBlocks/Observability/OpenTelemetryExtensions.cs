using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Observability;

public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Configures OpenTelemetry tracing, metrics, and logging for a microservice.
    /// Exports to OTLP endpoint (Jaeger) and console (development).
    /// </summary>
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(opts =>
                {
                    opts.RecordException = true;
                    opts.Filter = httpContext =>
                        !httpContext.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation(opts => opts.RecordException = true)
                .AddGrpcClientInstrumentation()
                .AddSource("MassTransit")
                .AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint)));

        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(opts =>
            {
                opts.IncludeFormattedMessage = true;
                opts.IncludeScopes = true;
                opts.AddOtlpExporter(exporterOpts =>
                    exporterOpts.Endpoint = new Uri(otlpEndpoint));
            });
        });

        return services;
    }
}
