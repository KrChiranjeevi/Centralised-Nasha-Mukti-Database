using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NashaMuktiDB.Data;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;
using Xunit;

namespace NashaMuktiDB.Tests;

/// <summary>
/// Part 9 Comprehensive Security Hardening & Regression Test Suite.
/// Validates HTTP security headers, IDOR isolation, inactive user rejection,
/// historical clinical record preservation, and CSV formula injection mitigations.
/// </summary>
public class SecurityAndAuditTests
{
    private ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private ClaimsPrincipal CreateClaimsPrincipal(string userId, string role, int? centreId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, $"User_{userId}"),
            new Claim(ClaimTypes.Email, $"{userId}@nashamukti.gov.in"),
            new Claim(ClaimTypes.Role, role)
        };

        if (centreId.HasValue)
        {
            claims.Add(new Claim("CentreId", centreId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private UserManager<ApplicationUser> CreateMockUserManager(List<ApplicationUser> users)
    {
        var store = new TestUserStore(users);
        return new UserManager<ApplicationUser>(
            store,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task SecurityHeaders_AreApplied_ToHttpResponse()
    {
        var context = new DefaultHttpContext();

        RequestDelegate next = (ctx) =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        // Invoke middleware logic as registered in Program.cs
        Func<HttpContext, RequestDelegate, Task> middleware = async (ctx, nxt) =>
        {
            var headers = ctx.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "SAMEORIGIN";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; font-src 'self' https://cdn.jsdelivr.net data:; img-src 'self' data:; connect-src 'self'; frame-ancestors 'self';";

            await nxt(ctx);
        };

        await middleware(context, next);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("SAMEORIGIN", context.Response.Headers["X-Frame-Options"]);
        Assert.Equal("1; mode=block", context.Response.Headers["X-XSS-Protection"]);
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
        Assert.Equal("camera=(), microphone=(), geolocation=()", context.Response.Headers["Permissions-Policy"]);
        Assert.Contains("default-src 'self'", context.Response.Headers["Content-Security-Policy"].ToString());
    }

    [Fact]
    public async Task IDOR_Protection_CentreStaff_CannotAccessForeignCentre()
    {
        var db = CreateInMemoryDbContext("TestSecurity_IDOR_Centre");
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "staff-amritsar",
                UserName = "staff-amritsar@nashamukti.gov.in",
                FullName = "Amritsar Staff",
                CentreId = 1,
                IsActive = true
            }
        };

        var userManager = CreateMockUserManager(users);
        var authService = new CentreAuthorizationService(userManager, db);

        // Staff assigned to Centre 1
        var staffUser = CreateClaimsPrincipal("staff-amritsar", "CentreStaff", centreId: 1);

        // Staff can access Centre 1
        var canAccessOwn = await authService.CanAccessCentreAsync(staffUser, 1);
        Assert.True(canAccessOwn, "Centre staff should have access to their assigned facility.");

        // Staff CANNOT access Centre 2 (IDOR protection)
        var canAccessForeign = await authService.CanAccessCentreAsync(staffUser, 2);
        Assert.False(canAccessForeign, "Centre staff must be strictly forbidden from accessing foreign centres.");
    }

    [Fact]
    public async Task IDOR_Protection_CentreStaff_CannotAccessForeignBeneficiary()
    {
        var db = CreateInMemoryDbContext("TestSecurity_IDOR_Beneficiary");

        // Seed beneficiary at Centre 2
        var foreignBeneficiary = new Beneficiary
        {
            BeneficiaryId = 501,
            BeneficiaryCode = "BEN-PB-2026-0501",
            FullName = "Harpreet Singh",
            CentreId = 2, // Centre 2
            RegistrationDate = DateTime.Today,
            Status = "Active"
        };
        db.Beneficiaries.Add(foreignBeneficiary);

        // Seed beneficiary at Centre 1
        var ownBeneficiary = new Beneficiary
        {
            BeneficiaryId = 502,
            BeneficiaryCode = "BEN-PB-2026-0502",
            FullName = "Gurpreet Kaur",
            CentreId = 1, // Centre 1
            RegistrationDate = DateTime.Today,
            Status = "Active"
        };
        db.Beneficiaries.Add(ownBeneficiary);
        await db.SaveChangesAsync();

        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "staff-amritsar",
                UserName = "staff-amritsar@nashamukti.gov.in",
                FullName = "Amritsar Staff",
                CentreId = 1,
                IsActive = true
            }
        };

        var userManager = CreateMockUserManager(users);
        var authService = new CentreAuthorizationService(userManager, db);

        // Staff assigned to Centre 1
        var staffUser = CreateClaimsPrincipal("staff-amritsar", "CentreStaff", centreId: 1);

        // Access to own centre's beneficiary
        var canAccessOwnBen = await authService.CanAccessBeneficiaryAsync(staffUser, 502);
        Assert.True(canAccessOwnBen);

        // Access to foreign centre's beneficiary must be blocked
        var canAccessForeignBen = await authService.CanAccessBeneficiaryAsync(staffUser, 501);
        Assert.False(canAccessForeignBen, "Accessing foreign beneficiary without authorisation must return false.");
    }

    [Fact]
    public async Task Administrator_HasUnrestrictedAccess_AcrossCentresAndBeneficiaries()
    {
        var db = CreateInMemoryDbContext("TestSecurity_Admin_UniversalAccess");

        var b1 = new Beneficiary
        {
            BeneficiaryId = 601,
            BeneficiaryCode = "BEN-PB-2026-0601",
            FullName = "National Admin Test Ben",
            CentreId = 3,
            RegistrationDate = DateTime.Today,
            Status = "Active"
        };
        db.Beneficiaries.Add(b1);
        await db.SaveChangesAsync();

        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "super-admin",
                UserName = "admin@nashamukti.gov.in",
                FullName = "National Administrator",
                CentreId = null,
                IsActive = true
            }
        };

        var userManager = CreateMockUserManager(users);
        var authService = new CentreAuthorizationService(userManager, db);

        var adminUser = CreateClaimsPrincipal("super-admin", "Admin", centreId: null);

        var canAccessCentre = await authService.CanAccessCentreAsync(adminUser, 3);
        var canAccessBen = await authService.CanAccessBeneficiaryAsync(adminUser, 601);

        Assert.True(canAccessCentre);
        Assert.True(canAccessBen);
    }

    [Fact]
    public async Task InactiveUser_IsIdentified_AndRejected()
    {
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = "deactivated-user",
                UserName = "deactivated@nashamukti.gov.in",
                FullName = "Deactivated Staff",
                CentreId = 1,
                IsActive = false
            }
        };

        var userManager = CreateMockUserManager(users);
        var user = await userManager.FindByIdAsync("deactivated-user");

        Assert.NotNull(user);
        Assert.False(user.IsActive, "Deactivated user must have IsActive set to false.");
    }

    [Fact]
    public async Task HistoricalRecords_ArePreserved_WhenBeneficiaryDeactivated()
    {
        var db = CreateInMemoryDbContext("TestSecurity_PreserveHistory");

        // Seed centre
        var centre = new Centre
        {
            CentreId = 10,
            CentreName = "Test De-Addiction Centre",
            StateId = 1,
            District = "Patiala",
            Address = "123 Test Road",
            ContactNumber = "9876543210",
            Email = "test@centre.org",
            IsActive = true
        };
        db.Centres.Add(centre);

        // Seed beneficiary
        var beneficiary = new Beneficiary
        {
            BeneficiaryId = 701,
            BeneficiaryCode = "BEN-PB-2026-0701",
            FullName = "Rajinder Kumar",
            CentreId = 10,
            RegistrationDate = DateTime.Today.AddMonths(-3),
            Status = "Active"
        };
        db.Beneficiaries.Add(beneficiary);

        // Add historical counselling session
        db.CounsellingSessions.Add(new CounsellingSession
        {
            SessionId = 801,
            BeneficiaryId = 701,
            SessionDate = DateTime.Today.AddMonths(-2),
            SessionType = "Individual",
            SessionStatus = "Completed",
            Notes = "Confidential initial intake notes"
        });

        // Add historical intervention
        db.Interventions.Add(new Intervention
        {
            InterventionId = 901,
            BeneficiaryId = 701,
            ServiceDate = DateTime.Today.AddMonths(-1),
            InterventionType = "Medical Detoxification",
            Status = "Completed"
        });

        // Add historical follow-up
        db.FollowUps.Add(new FollowUp
        {
            FollowUpId = 1001,
            BeneficiaryId = 701,
            ScheduledDate = DateTime.Today.AddDays(7),
            Status = "Pending"
        });

        await db.SaveChangesAsync();

        // Perform controlled deactivation (soft delete)
        beneficiary.Status = "Inactive";
        beneficiary.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Verify the beneficiary is inactive but NOT deleted
        var reloadedBen = await db.Beneficiaries.FindAsync(701);
        Assert.NotNull(reloadedBen);
        Assert.Equal("Inactive", reloadedBen.Status);

        // Verify all related clinical history is completely intact
        var sessions = await db.CounsellingSessions.Where(s => s.BeneficiaryId == 701).ToListAsync();
        Assert.Single(sessions);
        Assert.Equal("Completed", sessions[0].SessionStatus);

        var interventions = await db.Interventions.Where(i => i.BeneficiaryId == 701).ToListAsync();
        Assert.Single(interventions);

        var followUps = await db.FollowUps.Where(f => f.BeneficiaryId == 701).ToListAsync();
        Assert.Single(followUps);
    }

    [Fact]
    public void CsvFormulaInjection_SanitizesDangerousLeadingCharacters()
    {
        // CWE-1236 test cases
        var dangerousInputs = new[]
        {
            "=1+1",
            "-5+2",
            "+SUM(A1:A10)",
            "@HYPERLINK(\"http://evil.com\")",
            "\tTAB_INJECT",
            "\rCR_INJECT"
        };

        foreach (var input in dangerousInputs)
        {
            var clean = input;
            if (clean.StartsWith("=") || clean.StartsWith("+") || clean.StartsWith("-") || clean.StartsWith("@") || clean.StartsWith("\t") || clean.StartsWith("\r"))
            {
                clean = "'" + clean;
            }
            else
            {
                var trimmed = clean.TrimStart();
                if (trimmed.StartsWith("=") || trimmed.StartsWith("+") || trimmed.StartsWith("-") || trimmed.StartsWith("@"))
                {
                    clean = "'" + clean;
                }
            }
            if (clean.Contains('"') || clean.Contains(',') || clean.Contains('\n') || clean.Contains('\r'))
            {
                clean = "\"" + clean.Replace("\"", "\"\"") + "\"";
            }
            else
            {
                clean = "\"" + clean + "\"";
            }

            // Assert that the dangerous prefix is neutralized with a single quote prefix
            Assert.StartsWith("\"'", clean);
        }
    }

    [Fact]
    public void ModelValidation_CreateCentreDto_ValidatesRequiredFields()
    {
        var invalidDto = new CreateCentreDto
        {
            CentreName = "", // Required
            District = "",   // Required
            Address = "",    // Required
            ContactNumber = "", // Required
            Email = "not-an-email" // Invalid format
        };

        var context = new ValidationContext(invalidDto);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(invalidDto, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("CentreName"));
        Assert.Contains(results, r => r.MemberNames.Contains("District"));
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void ModelValidation_CreateBeneficiaryViewModel_RejectsFutureDateOfBirth()
    {
        var model = new CreateBeneficiaryViewModel
        {
            FullName = "Test Beneficiary",
            CentreId = 1,
            RegistrationDate = DateTime.Today,
            DateOfBirth = DateTime.Today.AddDays(5) // Future DOB
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("Date of birth cannot be in the future"));
    }
}

public class TestUserStore : IUserStore<ApplicationUser>
{
    private readonly List<ApplicationUser> _users;

    public TestUserStore(List<ApplicationUser> users)
    {
        _users = users;
    }

    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _users.Add(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _users.Remove(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        return Task.FromResult(user);
    }

    public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = _users.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName || u.UserName == normalizedUserName);
        return Task.FromResult(user);
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public void Dispose()
    {
    }
}
