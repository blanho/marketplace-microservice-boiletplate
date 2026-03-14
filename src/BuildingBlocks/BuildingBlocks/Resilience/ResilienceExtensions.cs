using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace BuildingBlocks.Resilience;

public static class ResilienceExtensions
{
    /// <summary>
    /// Adds a named HttpClient with Polly resilience policies:
    /// Retry (exponential backoff) → Circuit Breaker → Timeout.
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string name,
        string baseAddress,
        int retryCount = 3,
        int circuitBreakerThreshold = 5,
        int timeoutSeconds = 30)
    {
        return services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds + 5); // Outer timeout > Polly timeout
            })
            .AddPolicyHandler(GetRetryPolicy(retryCount))
            .AddPolicyHandler(GetCircuitBreakerPolicy(circuitBreakerThreshold))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(timeoutSeconds));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, _) =>
                {
                    // Structured log emitted by the consuming service's logger
                    Console.WriteLine(
                        $"[Polly Retry] Attempt {retryAttempt} after {timespan.TotalSeconds}s — " +
                        $"Status: {outcome.Result?.StatusCode}");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int threshold)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: threshold,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (_, duration) =>
                    Console.WriteLine($"[Polly CB] Circuit OPEN for {duration.TotalSeconds}s"),
                onReset: () =>
                    Console.WriteLine("[Polly CB] Circuit CLOSED"),
                onHalfOpen: () =>
                    Console.WriteLine("[Polly CB] Circuit HALF-OPEN"));
    }
}
