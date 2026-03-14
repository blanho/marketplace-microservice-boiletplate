using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "[{ExceptionType}] {Message} | Path: {Path}",
            exception.GetType().Name,
            exception.Message,
            context.Request.Path);

        var (statusCode, title) = exception switch
        {
            ValidationException    => (StatusCodes.Status400BadRequest,  "Validation Error"),
            BadRequestException    => (StatusCodes.Status400BadRequest,  "Bad Request"),
            NotFoundException      => (StatusCodes.Status404NotFound,    "Not Found"),
            InternalServerException => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
            _                      => (StatusCodes.Status500InternalServerError, "Unexpected Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = exception.Message,
            Instance = $"{context.Request.Method} {context.Request.Path}"
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());
        }

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}

