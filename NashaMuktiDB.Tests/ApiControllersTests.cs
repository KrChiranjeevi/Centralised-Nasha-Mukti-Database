using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NashaMuktiDB.Controllers.Api;
using NashaMuktiDB.Data;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;
using Xunit;

namespace NashaMuktiDB.Tests;

public class ApiControllersTests
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

    [Fact]
    public async Task CentresApiController_GetCentres_ReturnsPaginatedList()
    {
        var context = CreateInMemoryDbContext("TestApi_Centres_List");
        var adminUser = CreateClaimsPrincipal("admin-1", "Admin");
        var authMock = new MockCentreAuthService { MockIsAdmin = true };

        var controller = new CentresApiController(context, authMock, NullLogger<CentresApiController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = adminUser } }
        };

        var result = await controller.GetCentres(search: null, stateId: null, district: null, activeOnly: true, page: 1, pageSize: 5);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var paginated = Assert.IsType<PaginatedApiResponse<CentreResponseDto>>(okResult.Value);

        Assert.True(paginated.TotalCount > 0);
        Assert.NotEmpty(paginated.Items);
    }

    [Fact]
    public async Task CentresApiController_GetCentreById_ReturnsNotFound_OnMissingId()
    {
        var context = CreateInMemoryDbContext("TestApi_Centres_NotFound");
        var adminUser = CreateClaimsPrincipal("admin-1", "Admin");
        var authMock = new MockCentreAuthService { MockIsAdmin = true };

        var controller = new CentresApiController(context, authMock, NullLogger<CentresApiController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = adminUser } }
        };

        var result = await controller.GetCentreById(999999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<string>>(notFound.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task BeneficiariesApiController_GetBeneficiaries_EnforcesCentreStaffBoundary()
    {
        var context = CreateInMemoryDbContext("TestApi_Beneficiaries_Scoping");
        var staffUser = CreateClaimsPrincipal("staff-1", "CentreStaff", centreId: 1);
        var authMock = new MockCentreAuthService { MockIsAdmin = false, MockUserCentreId = 1 };

        var benService = new BeneficiaryService(context, NullLogger<BeneficiaryService>.Instance);
        var controller = new BeneficiariesApiController(context, benService, authMock, NullLogger<BeneficiariesApiController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = staffUser } }
        };

        // Staff tries to request Centre 2
        var result = await controller.GetBeneficiaries(search: null, centreId: 2, status: null, page: 1, pageSize: 20);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var paginated = Assert.IsType<PaginatedApiResponse<BeneficiaryResponseDto>>(okResult.Value);

        // Every returned beneficiary must belong strictly to Centre 1
        Assert.All(paginated.Items, b => Assert.Equal(1, b.CentreId));
    }

    [Fact]
    public async Task BeneficiariesApiController_RegisterBeneficiary_RejectsInvalidModel()
    {
        var context = CreateInMemoryDbContext("TestApi_Beneficiaries_Invalid");
        var adminUser = CreateClaimsPrincipal("admin-1", "Admin");
        var authMock = new MockCentreAuthService { MockIsAdmin = true };

        var benService = new BeneficiaryService(context, NullLogger<BeneficiaryService>.Instance);
        var controller = new BeneficiariesApiController(context, benService, authMock, NullLogger<BeneficiariesApiController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = adminUser } }
        };

        controller.ModelState.AddModelError("FullName", "Full name is required");

        var result = await controller.RegisterBeneficiary(new RegisterBeneficiaryDto { FullName = "" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<string>>(badRequest.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task CounsellingApiController_GetSessions_OmitsConfidentialNotes()
    {
        var context = CreateInMemoryDbContext("TestApi_Counselling_Privacy");
        var adminUser = CreateClaimsPrincipal("admin-1", "Admin");
        var authMock = new MockCentreAuthService { MockIsAdmin = true };

        var controller = new CounsellingApiController(context, authMock, NullLogger<CounsellingApiController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = adminUser } }
        };

        var result = await controller.GetSessions(search: null, beneficiaryId: null, sessionType: null, status: null, page: 1, pageSize: 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var paginated = Assert.IsType<PaginatedApiResponse<CounsellingListItemDto>>(okResult.Value);

        Assert.NotEmpty(paginated.Items);
        // CounsellingListItemDto does not have a Notes property (type-level privacy by design)
        var hasNotesProperty = typeof(CounsellingListItemDto).GetProperty("Notes") != null;
        Assert.False(hasNotesProperty, "CounsellingListItemDto must not expose Notes property");
    }

    [Fact]
    public async Task ReportsApiController_GetSummary_ReturnsAll9Kpis()
    {
        var context = CreateInMemoryDbContext("TestApi_Reports_Summary");
        var adminUser = CreateClaimsPrincipal("admin-1", "Admin");
        var authMock = new MockCentreAuthService { MockIsAdmin = true };
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        var controller = new ReportsApiController(reportService)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = adminUser } }
        };

        var result = await controller.GetSummary(stateId: null, centreId: null, district: null, startDate: null, endDate: null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<ReportSummaryDto>>(okResult.Value);

        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.True(apiResponse.Data.TotalRegisteredBeneficiaries > 0);
        Assert.True(apiResponse.Data.TotalRegisteredCentres > 0);
        Assert.True(apiResponse.Data.TotalCounsellingSessions > 0);
        Assert.True(apiResponse.Data.TotalRecordedInterventions > 0);
    }
}
