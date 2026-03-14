using System.Threading.RateLimiting;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// ── YARP Reverse Proxy ───────────────────────────────────────────────────────
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── Rate Limiting ────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── Health Checks (aggregates downstream services) ───────────────────────────
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("http://localhost:6000/health"), name: "catalog-service")
    .AddUrlGroup(new Uri("http://localhost:6001/health"), name: "basket-service")
    .AddUrlGroup(new Uri("http://localhost:6003/health"), name: "order-service");

// ── OpenTelemetry ────────────────────────────────────────────────────────────
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ApiGateway"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(opts =>
        {
            opts.RecordException = true;
            opts.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
        })
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint)));

var app = builder.Build();

// ── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseRateLimiter();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false, // Liveness
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true, // Readiness (includes downstream checks)
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapReverseProxy();

app.Run();
