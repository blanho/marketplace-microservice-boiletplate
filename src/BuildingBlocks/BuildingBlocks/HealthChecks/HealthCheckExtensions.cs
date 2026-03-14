using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace BuildingBlocks.HealthChecks;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Maps both /health (liveness) and /health/ready (readiness) endpoints
    /// with JSON-formatted responses.
    /// </summary>
    public static WebApplication UseCustomHealthChecks(this WebApplication app)
    {
        // Liveness — is the process alive?
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false, // No dependency checks — just "are we responding?"
            ResponseWriter = WriteResponse
        });

        // Readiness — are all dependencies available?
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = _ => true, // All registered health checks
            ResponseWriter = WriteResponse
        });

        return app;
    }

    private static async Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString(),
                exception = e.Value.Exception?.Message,
                data = e.Value.Data.Count > 0 ? e.Value.Data : null
            })
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(json);
    }
}
