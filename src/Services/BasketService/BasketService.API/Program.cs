using BuildingBlocks.HealthChecks;
using BuildingBlocks.Observability;
using Discount.Grpc;
using Messaging.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database")!);
    options.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

// Repository — Scrutor decorates BasketRepository with CacheBasketRepository transparently
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CacheBasketRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// ── gRPC Clients ─────────────────────────────────────────────────────────────
builder.Services.AddGrpcClient<DiscountService.DiscountServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
});

// ── MassTransit + RabbitMQ ───────────────────────────────────────────────────
builder.Services.AddMessageBroker(builder.Configuration);

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── OpenTelemetry ────────────────────────────────────────────────────────────
builder.Services.AddObservability(builder.Configuration, "BasketService");

// ── Health Checks ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.MapCarter();
app.UseCustomHealthChecks();

app.Run();





