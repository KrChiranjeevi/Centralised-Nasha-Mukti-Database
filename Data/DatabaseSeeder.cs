using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Models;

namespace NashaMuktiDB.Data;

/// <summary>
/// Provides runtime database initialization and seeding for Identity Roles and Users.
/// Safe to execute repeatedly without duplicating data.
/// </summary>
public static class DatabaseSeeder
{
    public const string RoleAdmin = "Admin";
    public const string RoleCentreStaff = "CentreStaff";

    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var hostEnv = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();

            // 1. Initialize Database Schema
            if (!context.Database.IsRelational())
            {
                logger.LogInformation("Using In-Memory Database provider. Ensuring database is created with seed data...");
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                bool canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    logger.LogWarning("SQL Server is currently unreachable. Identity seeding deferred until connection is established.");
                    return;
                }

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending EF Core migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    logger.LogInformation("EF Core migrations applied successfully.");
                }
            }

            // 2. Seed Roles
            await EnsureRoleAsync(roleManager, RoleAdmin, logger);
            await EnsureRoleAsync(roleManager, RoleCentreStaff, logger);

            // 3. Seed Administrators
            await EnsureAdminUserAsync(userManager, "admin@nmd.gov.in", "Admin@NMD2026!", "National Administrator", logger);
            await EnsureAdminUserAsync(userManager, "admin@nashamukti.gov.in", "Admin@12345", "National Portal Admin", logger);

            // 4. Seed Sample Centre Staff (Development/Staging Only)
            if (!hostEnv.IsProduction())
            {
                await EnsureStaffUserAsync(userManager, "staff.amritsar@nmd.gov.in", "Staff@NMD2026!", "Amritsar Centre Staff", 1, logger);
                await EnsureStaffUserAsync(userManager, "staff.patna@nmd.gov.in", "Staff@NMD2026!", "Patna Centre Staff", 3, logger);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database migration or Identity seeding: {Message}", ex.Message);
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName, ILogger logger)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                logger.LogInformation("Role created: {Role}", roleName);
            }
        }
    }

    private static async Task EnsureAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing == null)
        {
            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                CentreId = null,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, RoleAdmin);
                logger.LogInformation("Administrator account created: {Email}", email);
            }
            else
            {
                logger.LogError("Failed to create Administrator {Email}: {Errors}",
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task EnsureStaffUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        int centreId,
        ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing == null)
        {
            var staff = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                CentreId = centreId,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(staff, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(staff, RoleCentreStaff);
                logger.LogInformation("CentreStaff account created: {Email} (CentreId = {CentreId})", email, centreId);
            }
            else
            {
                logger.LogError("Failed to create Staff {Email}: {Errors}",
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
