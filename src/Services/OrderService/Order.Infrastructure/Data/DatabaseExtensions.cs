using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Order.Infrastructure.Data;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderDbContext>>();

        logger.LogInformation("Applying Order database migrations…");
        await context.Database.MigrateAsync();
        logger.LogInformation("Order database is up to date.");
    }
}
