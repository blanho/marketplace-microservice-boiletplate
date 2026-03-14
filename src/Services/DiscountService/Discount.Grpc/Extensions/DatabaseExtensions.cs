using Discount.Grpc.Data;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DiscountContext>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Applying EF Core migrations…");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database is up to date.");
    }
}
