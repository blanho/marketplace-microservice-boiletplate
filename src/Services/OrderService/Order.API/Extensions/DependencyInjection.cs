using BuildingBlocks.Behaviours;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.HealthChecks;
using BuildingBlocks.Observability;
using Carter;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Order.Infrastructure.Data;

namespace Order.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Carter (Minimal API endpoint modules) ────────────────────────────
        services.AddCarter();

        // ── MediatR + pipeline behaviours ────────────────────────────────────
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(
                typeof(Program).Assembly,
                typeof(Order.Application.Orders.Commands.CreateOrder.CreateOrderCommand).Assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(
            typeof(Order.Application.Orders.Commands.CreateOrder.CreateOrderValidator).Assembly);

        // ── Exception handling ───────────────────────────────────────────────
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddProblemDetails();

        // ── Swagger ──────────────────────────────────────────────────────────
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // ── OpenTelemetry ────────────────────────────────────────────────────
        services.AddObservability(configuration, "OrderService");

        // ── Health Checks ────────────────────────────────────────────────────
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        return services;
    }

    public static async Task<WebApplication> UseApiServices(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            await app.InitialiseDatabaseAsync();
        }

        app.UseExceptionHandler();
        app.MapCarter();
        app.UseCustomHealthChecks();

        return app;
    }
}