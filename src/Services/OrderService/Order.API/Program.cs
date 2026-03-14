using Order.API.Extensions;
using Order.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureService(builder.Configuration);

var app = builder.Build();

await app.UseApiServices();

await app.RunAsync();

