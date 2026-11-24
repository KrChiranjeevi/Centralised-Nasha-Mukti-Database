using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NashaMuktiDB.Controllers;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;
using Xunit;

namespace NashaMuktiDB.Tests;

/// <summary>
/// Unit and Workflow tests for Registration Account Requests, Admin Review & Approval,
/// CentreStaff data isolation, and secure password establishment.
/// </summary>
public class AccountRequestWorkflowTests
{
    private ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        // Seed State & Centre
        if (!context.States.Any())
        {
            var state = new State { StateId = 1, StateName = "Punjab", IsActive = true };
            context.States.Add(state);
            context.SaveChanges();
        }

        if (!context.Centres.Any())
        {
            var centre = new Centre
            {
                CentreId = 1,
                CentreName = "Amritsar De-Addiction Centre",
                StateId = 1,
                District = "Amritsar",
                Address = "Civil Hospital Road",
                ContactNumber = "+91 183 2223344",
                Email = "amritsar@nmd.gov.in",
                IsActive = true
            };
            context.Centres.Add(centre);
            context.SaveChanges();
        }

        return context;
    }

    private UserManager<ApplicationUser> CreateTestUserManager(List<ApplicationUser> userStore)
    {
        var store = new ComprehensiveTestUserStore(userStore);
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var userValidators = new List<IUserValidator<ApplicationUser>> { new UserValidator<ApplicationUser>() };
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new PasswordValidator<ApplicationUser>() };
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();

        return new UserManager<ApplicationUser>(
            store,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);
    }

    private SignInManager<ApplicationUser> CreateTestSignInManager(UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var claimsFactory = new UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(
            userManager,
            new RoleManager<IdentityRole>(new TestRoleStore(), null!, null!, null!, null!),
            Options.Create(new IdentityOptions()));

        return new SignInManager<ApplicationUser>(
            userManager,
            contextAccessor,
            claimsFactory,
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<ApplicationUser>>.Instance,
            null!,
            null!);
    }

    [Fact]
    public void RequestAccountViewModel_ValidationAttributes_RequireMandatoryFields()
    {
        // Arrange
        var model = new RequestAccountViewModel
        {
            FullName = "", // Missing
            Email = "invalid-email-format",
            RequestedCentreId = null,
            ReasonForAccess = "Short" // Less than 10 characters
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);

        // Act
        var isValid = Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(RequestAccountViewModel.FullName)));
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(RequestAccountViewModel.Email)));
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(RequestAccountViewModel.RequestedCentreId)));
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(RequestAccountViewModel.ReasonForAccess)));
    }

    [Fact]
    public void ApproveAccountRequestViewModel_ValidationAttributes_EnforceSecurity()
    {
        // Arrange
        var model = new ApproveAccountRequestViewModel
        {
            AssignedRole = "CentreStaff",
            AssignedCentreId = 1,
            TemporaryPassword = "Short", // Less than 8 characters
            ConfirmPassword = "DifferentPassword123!" // Mismatched
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);

        // Act
        var isValid = Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(ApproveAccountRequestViewModel.TemporaryPassword)));
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(ApproveAccountRequestViewModel.ConfirmPassword)));
    }

    [Fact]
    public async Task RequestAccount_ValidSubmission_CreatesPendingRecordInDatabase()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(RequestAccount_ValidSubmission_CreatesPendingRecordInDatabase));
        var userStore = new List<ApplicationUser>();
        var userManager = CreateTestUserManager(userStore);
        var signInManager = CreateTestSignInManager(userManager);

        var controller = new AccountController(
            signInManager,
            userManager,
            context,
            NullLogger<AccountController>.Instance);

        var requestModel = new RequestAccountViewModel
        {
            FullName = "Dr. Manpreet Singh",
            Email = "manpreet.singh@nmd.gov.in",
            ContactNumber = "+91 98765 11223",
            RequestedCentreId = 1,
            ReasonForAccess = "Clinical Psychologist conducting intake evaluations and relapse prevention sessions."
        };

        // Act
        var result = await controller.RequestAccount(requestModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AccountController.RequestConfirmation), redirectResult.ActionName);

        var saved = await context.AccountRequests.FirstOrDefaultAsync(r => r.Email == "manpreet.singh@nmd.gov.in");
        Assert.NotNull(saved);
        Assert.Equal(nameof(AccountRequestStatus.Pending), saved.Status);
        Assert.Equal(1, saved.RequestedCentreId);
        Assert.Equal("Dr. Manpreet Singh", saved.FullName);
    }

    [Fact]
    public async Task RequestAccount_DuplicatePendingSubmission_RejectsWithModelError()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(RequestAccount_DuplicatePendingSubmission_RejectsWithModelError));
        context.AccountRequests.Add(new AccountRequest
        {
            FullName = "Existing Applicant",
            Email = "duplicate@nmd.gov.in",
            RequestedCentreId = 1,
            ReasonForAccess = "Initial access request for Kendra records.",
            Status = nameof(AccountRequestStatus.Pending),
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var userStore = new List<ApplicationUser>();
        var userManager = CreateTestUserManager(userStore);
        var signInManager = CreateTestSignInManager(userManager);

        var controller = new AccountController(
            signInManager,
            userManager,
            context,
            NullLogger<AccountController>.Instance);

        var duplicateModel = new RequestAccountViewModel
        {
            FullName = "Second Attempt",
            Email = "duplicate@nmd.gov.in",
            RequestedCentreId = 1,
            ReasonForAccess = "Another access attempt with same email address."
        };

        // Act
        var result = await controller.RequestAccount(duplicateModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(RequestAccountViewModel.Email)));
    }

    [Fact]
    public async Task PendingApplicant_CannotSignIn_ReturnsPendingExplanationMessage()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(PendingApplicant_CannotSignIn_ReturnsPendingExplanationMessage));
        context.AccountRequests.Add(new AccountRequest
        {
            FullName = "Awaiting Verification",
            Email = "pending.user@nmd.gov.in",
            RequestedCentreId = 1,
            ReasonForAccess = "Awaiting admin signoff.",
            Status = nameof(AccountRequestStatus.Pending),
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var userStore = new List<ApplicationUser>(); // No Identity user created yet
        var userManager = CreateTestUserManager(userStore);
        var signInManager = CreateTestSignInManager(userManager);

        var controller = new AccountController(
            signInManager,
            userManager,
            context,
            NullLogger<AccountController>.Instance);

        var loginModel = new LoginViewModel
        {
            Email = "pending.user@nmd.gov.in",
            Password = "AnyPassword123!"
        };

        // Act
        var result = await controller.Login(loginModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        var errorMessages = controller.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        Assert.Contains(errorMessages, msg => msg.Contains("pending administrative review"));
    }

    [Fact]
    public async Task Admin_ApproveRequest_CreatesUserWithAssignedRoleAndCentre()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(Admin_ApproveRequest_CreatesUserWithAssignedRoleAndCentre));
        var pendingReq = new AccountRequest
        {
            AccountRequestId = 101,
            FullName = "Dr. Ananya Roy",
            Email = "ananya.roy@nmd.gov.in",
            RequestedCentreId = 1,
            ReasonForAccess = "Psychiatrist leading outpatient clinic.",
            Status = nameof(AccountRequestStatus.Pending),
            CreatedAt = DateTime.UtcNow
        };
        context.AccountRequests.Add(pendingReq);
        await context.SaveChangesAsync();

        var userStore = new List<ApplicationUser>();
        var adminUser = new ApplicationUser
        {
            Id = "admin-guid-001",
            UserName = "admin@nmd.gov.in",
            Email = "admin@nmd.gov.in",
            FullName = "National Administrator",
            IsActive = true
        };
        userStore.Add(adminUser);

        var userManager = CreateTestUserManager(userStore);
        var roleManager = new RoleManager<IdentityRole>(new TestRoleStore(), null!, null!, null!, null!);

        var controller = new AdminController(
            userManager,
            roleManager,
            context,
            NullLogger<AdminController>.Instance);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, adminUser.Id),
            new(ClaimTypes.Name, adminUser.UserName),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());

        var approvalModel = new ApproveAccountRequestViewModel
        {
            AccountRequestId = 101,
            AssignedRole = DatabaseSeeder.RoleCentreStaff,
            AssignedCentreId = 1,
            TemporaryPassword = "TempPassword@2026!",
            ConfirmPassword = "TempPassword@2026!",
            RequirePasswordChange = true,
            AdminNotes = "Approved after telephone credential verification."
        };

        // Act
        var result = await controller.ApproveRequest(approvalModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.AccountRequests), redirectResult.ActionName);

        var updatedReq = await context.AccountRequests.FindAsync(101);
        Assert.NotNull(updatedReq);
        Assert.Equal(nameof(AccountRequestStatus.Approved), updatedReq.Status);
        Assert.NotNull(updatedReq.ApprovedUserId);

        var createdUser = userStore.FirstOrDefault(u => u.Email == "ananya.roy@nmd.gov.in");
        Assert.NotNull(createdUser);
        Assert.Equal("Dr. Ananya Roy", createdUser.FullName);
        Assert.Equal(1, createdUser.CentreId);
        Assert.True(createdUser.MustChangePassword);
    }

    [Fact]
    public async Task Admin_ApproveStaff_WithoutAssignedCentre_FailsValidation()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(Admin_ApproveStaff_WithoutAssignedCentre_FailsValidation));
        var pendingReq = new AccountRequest
        {
            AccountRequestId = 102,
            FullName = "Staff Without Centre",
            Email = "nocentre@nmd.gov.in",
            RequestedCentreId = 1,
            ReasonForAccess = "Testing validation.",
            Status = nameof(AccountRequestStatus.Pending)
        };
        context.AccountRequests.Add(pendingReq);
        await context.SaveChangesAsync();

        var userStore = new List<ApplicationUser>();
        var userManager = CreateTestUserManager(userStore);
        var roleManager = new RoleManager<IdentityRole>(new TestRoleStore(), null!, null!, null!, null!);

        var controller = new AdminController(
            userManager,
            roleManager,
            context,
            NullLogger<AdminController>.Instance);

        var approvalModel = new ApproveAccountRequestViewModel
        {
            AccountRequestId = 102,
            AssignedRole = DatabaseSeeder.RoleCentreStaff,
            AssignedCentreId = null, // Missing centre for staff
            TemporaryPassword = "TempPassword@2026!",
            ConfirmPassword = "TempPassword@2026!"
        };

        // Act
        var result = await controller.ApproveRequest(approvalModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(ApproveAccountRequestViewModel.AssignedCentreId)));
    }

    [Fact]
    public async Task Admin_RejectRequest_MarksStatusAsRejectedWithAuditNotes()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(Admin_RejectRequest_MarksStatusAsRejectedWithAuditNotes));
        var req = new AccountRequest
        {
            AccountRequestId = 103,
            FullName = "Ineligible Applicant",
            Email = "ineligible@nmd.gov.in",
            RequestedCentreId = 1,
            ReasonForAccess = "Unauthorized external request.",
            Status = nameof(AccountRequestStatus.Pending)
        };
        context.AccountRequests.Add(req);
        await context.SaveChangesAsync();

        var adminUser = new ApplicationUser { Id = "admin-99", UserName = "admin@nmd.gov.in", Email = "admin@nmd.gov.in" };
        var userStore = new List<ApplicationUser> { adminUser };
        var userManager = CreateTestUserManager(userStore);
        var roleManager = new RoleManager<IdentityRole>(new TestRoleStore(), null!, null!, null!, null!);

        var controller = new AdminController(
            userManager,
            roleManager,
            context,
            NullLogger<AdminController>.Instance);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, adminUser.Id),
            new(ClaimTypes.Name, adminUser.UserName)
        };
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());

        // Act
        var result = await controller.RejectRequest(103, "Applicant is not on official Kendra roster.");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.AccountRequests), redirectResult.ActionName);

        var rejected = await context.AccountRequests.FindAsync(103);
        Assert.NotNull(rejected);
        Assert.Equal(nameof(AccountRequestStatus.Rejected), rejected.Status);
        Assert.Equal("Applicant is not on official Kendra roster.", rejected.AdminNotes);
        Assert.Equal("admin-99", rejected.ReviewedByUserId);
    }
}

public class MockTempDataProvider : ITempDataProvider
{
    private readonly Dictionary<string, object> _values = new();

    public IDictionary<string, object> LoadTempData(HttpContext context) => _values;

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
        _values.Clear();
        foreach (var kvp in values)
        {
            _values[kvp.Key] = kvp.Value;
        }
    }
}
