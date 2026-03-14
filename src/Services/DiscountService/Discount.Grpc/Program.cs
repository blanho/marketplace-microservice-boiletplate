using Discount.Grpc.Data;
using Discount.Grpc.Extensions;
using Discount.Grpc.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── EF Core ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<DiscountContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Database"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null)));

// ── Repository ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();

// ── gRPC ─────────────────────────────────────────────────────────────────────
builder.Services.AddGrpc();

// ── Health Checks ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

var app = builder.Build();

await app.InitialiseDatabaseAsync();

app.MapGrpcService<DiscountService>();

app.MapGet("/",
    () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.RunAsync();
