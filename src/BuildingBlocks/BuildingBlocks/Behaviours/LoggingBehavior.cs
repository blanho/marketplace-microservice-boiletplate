using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Behaviours;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestType = typeof(TRequest).FullName;

        logger.LogInformation(
            "[START] {RequestName} | Type: {RequestType}",
            requestName, requestType);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 3000)
            {
                logger.LogWarning(
                    "[PERFORMANCE] {RequestName} took {ElapsedMs}ms — exceeds 3s threshold.",
                    requestName, stopwatch.ElapsedMilliseconds);
            }

            logger.LogInformation(
                "[END] {RequestName} completed in {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "[ERROR] {RequestName} failed after {ElapsedMs}ms | Exception: {ExceptionType}",
                requestName, stopwatch.ElapsedMilliseconds, ex.GetType().Name);
            throw;
        }
    }
}

