using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Models;

namespace NashaMuktiDB.Data;

/// <summary>
/// Provides runtime database initialization and seeding for demonstration data.
/// Safe to run repeatedly without creating duplicates.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            // Verify if database can be connected
            bool canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogWarning("SQL Server is currently unreachable. Database seeding deferred until connection is established.");
                return;
            }

            // Apply any pending migrations automatically if connected
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending EF Core migrations...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("EF Core migrations applied successfully.");
            }

            // Verify tables and seed fallback (in case HasData was not applied)
            if (!await context.States.AnyAsync())
            {
                logger.LogInformation("Seeding fallback States...");
                await context.States.AddRangeAsync(
                    new State { StateName = "Punjab", IsActive = true },
                    new State { StateName = "Bihar", IsActive = true },
                    new State { StateName = "Maharashtra", IsActive = true },
                    new State { StateName = "Rajasthan", IsActive = true },
                    new State { StateName = "Delhi", IsActive = true }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("States seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database migration or seeding: {Message}", ex.Message);
        }
    }
}
