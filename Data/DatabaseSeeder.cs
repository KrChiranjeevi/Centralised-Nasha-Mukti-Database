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

            // 1. Check if database can be reached
            bool canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogWarning("SQL Server is currently unreachable. Identity seeding deferred until connection is established.");
                return;
            }

            // 2. Apply pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending EF Core migrations...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("EF Core migrations applied successfully.");
            }

            // 3. Seed Roles
            await EnsureRoleAsync(roleManager, RoleAdmin, logger);
            await EnsureRoleAsync(roleManager, RoleCentreStaff, logger);

            // 4. Seed Default Administrator
            var adminEmail = configuration["Security:DefaultAdminEmail"] ?? "admin@nmd.gov.in";
            var adminPassword = configuration["Security:DefaultAdminPassword"] 
                ?? Environment.GetEnvironmentVariable("NMD_ADMIN_PASSWORD") 
                ?? "Admin@NMD2026!";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "National Administrator",
                    CentreId = null, // Global administrator is not tied to a specific centre
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, RoleAdmin);
                    logger.LogInformation("Initial Administrator account created: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to create Administrator account: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // 5. Seed Sample Centre Staff 1 (Amritsar Centre, CentreId = 1)
            var staff1Email = "staff.amritsar@nmd.gov.in";
            if (await userManager.FindByEmailAsync(staff1Email) == null)
            {
                var staff1 = new ApplicationUser
                {
                    UserName = staff1Email,
                    Email = staff1Email,
                    FullName = "Amritsar Centre Staff",
                    CentreId = 1,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(staff1, "Staff@NMD2026!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staff1, RoleCentreStaff);
                    logger.LogInformation("Sample CentreStaff account created: {Email} (CentreId = 1)", staff1Email);
                }
            }

            // 6. Seed Sample Centre Staff 2 (Patna Centre, CentreId = 3)
            var staff2Email = "staff.patna@nmd.gov.in";
            if (await userManager.FindByEmailAsync(staff2Email) == null)
            {
                var staff2 = new ApplicationUser
                {
                    UserName = staff2Email,
                    Email = staff2Email,
                    FullName = "Patna Centre Staff",
                    CentreId = 3,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(staff2, "Staff@NMD2026!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staff2, RoleCentreStaff);
                    logger.LogInformation("Sample CentreStaff account created: {Email} (CentreId = 3)", staff2Email);
                }
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
}
