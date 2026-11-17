using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;
using Xunit;

namespace NashaMuktiDB.Tests;

public class ReportServiceTests
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

    private (ClaimsPrincipal AdminUser, ClaimsPrincipal StaffUser) CreateTestUsers()
    {
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "admin-1"),
            new Claim(ClaimTypes.Name, "AdminUser"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(adminClaims, "TestAuth"));

        var staffClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "staff-1"),
            new Claim(ClaimTypes.Name, "StaffUser"),
            new Claim(ClaimTypes.Role, "CentreStaff"),
            new Claim("CentreId", "1")
        };
        var staffUser = new ClaimsPrincipal(new ClaimsIdentity(staffClaims, "TestAuth"));

        return (adminUser, staffUser);
    }

    [Fact]
    public async Task DashboardMetrics_MatchExpectedDirectEntityCounts()
    {
        var context = CreateInMemoryDbContext("TestDb_MetricsMatch");
        var (adminUser, _) = CreateTestUsers();

        var authMock = new MockCentreAuthService();
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        var filter = new ReportsFilterViewModel();
        var result = await reportService.GetReportsAsync(adminUser, filter);

        // Seed data in EnsureCreated seeds 6 beneficiaries, 5 centres, 4 counselling sessions, 3 interventions, 3 followups
        var directBenCount = await context.Beneficiaries.CountAsync();
        var directCentreCount = await context.Centres.CountAsync();
        var directSessionCount = await context.CounsellingSessions.CountAsync();
        var directInterventionCount = await context.Interventions.CountAsync();

        Assert.Equal(directBenCount, result.Metrics.TotalRegisteredBeneficiaries);
        Assert.Equal(directCentreCount, result.Metrics.TotalRegisteredCentres);
        Assert.Equal(directSessionCount, result.Metrics.TotalCounsellingSessions);
        Assert.Equal(directInterventionCount, result.Metrics.TotalRecordedInterventions);
    }

    [Fact]
    public async Task NoCartesianInflation_WhenMultipleChildEntitiesExist()
    {
        var context = CreateInMemoryDbContext("TestDb_CartesianPrevention");
        var (adminUser, _) = CreateTestUsers();

        // Add 1 test centre
        var centre = new Centre
        {
            CentreId = 999,
            CentreName = "Test Isolation Facility",
            StateId = 1,
            District = "Test District",
            Address = "Test Road",
            ContactNumber = "9999999999",
            Email = "isolation@test.org",
            IsActive = true
        };
        context.Centres.Add(centre);

        // Add 1 beneficiary
        var ben = new Beneficiary
        {
            BeneficiaryId = 9991,
            BeneficiaryCode = "BEN-TEST-9991",
            CentreId = 999,
            FullName = "Multi Service Patient",
            RegistrationDate = DateTime.Today,
            Status = "Active"
        };
        context.Beneficiaries.Add(ben);

        // Add 3 Counselling sessions
        context.CounsellingSessions.AddRange(
            new CounsellingSession { SessionId = 99911, BeneficiaryId = 9991, SessionDate = DateTime.Today, SessionType = "Individual", SessionStatus = "Completed" },
            new CounsellingSession { SessionId = 99912, BeneficiaryId = 9991, SessionDate = DateTime.Today, SessionType = "Group", SessionStatus = "Completed" },
            new CounsellingSession { SessionId = 99913, BeneficiaryId = 9991, SessionDate = DateTime.Today, SessionType = "Family", SessionStatus = "Completed" }
        );

        // Add 4 Interventions
        context.Interventions.AddRange(
            new Intervention { InterventionId = 99921, BeneficiaryId = 9991, InterventionType = "Medical Detox", ServiceDate = DateTime.Today, Status = "Completed" },
            new Intervention { InterventionId = 99922, BeneficiaryId = 9991, InterventionType = "Pharmacotherapy", ServiceDate = DateTime.Today, Status = "Completed" },
            new Intervention { InterventionId = 99923, BeneficiaryId = 9991, InterventionType = "Rehab Support", ServiceDate = DateTime.Today, Status = "Completed" },
            new Intervention { InterventionId = 99924, BeneficiaryId = 9991, InterventionType = "Vocational", ServiceDate = DateTime.Today, Status = "Completed" }
        );

        // Add 2 Follow-ups
        context.FollowUps.AddRange(
            new FollowUp { FollowUpId = 99931, BeneficiaryId = 9991, ScheduledDate = DateTime.Today.AddDays(14), Status = "Pending" },
            new FollowUp { FollowUpId = 99932, BeneficiaryId = 9991, ScheduledDate = DateTime.Today.AddDays(28), Status = "Pending" }
        );

        await context.SaveChangesAsync();

        var authMock = new MockCentreAuthService();
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        var filter = new ReportsFilterViewModel { CentreId = 999 };
        var result = await reportService.GetReportsAsync(adminUser, filter);

        var facilityReport = result.CentreReports.First(c => c.CentreId == 999);

        // If a cartesian join bug existed, count would multiply: 3 * 4 * 2 = 24 rows!
        Assert.Equal(1, facilityReport.TotalBeneficiaries);
        Assert.Equal(3, facilityReport.TotalCounsellingSessions);
        Assert.Equal(4, facilityReport.TotalInterventions);
        Assert.Equal(2, facilityReport.TotalFollowUps);
    }

    [Fact]
    public async Task CentreStaff_CannotAccessOtherCentresData()
    {
        var context = CreateInMemoryDbContext("TestDb_StaffScoping");
        var (_, staffUser) = CreateTestUsers();

        // CentreStaff belongs to Centre 1
        var authMock = new MockCentreAuthService { MockUserCentreId = 1, MockIsAdmin = false };
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        // Attacker attempts to filter by Centre 2
        var spoofedFilter = new ReportsFilterViewModel { CentreId = 2 };
        var result = await reportService.GetReportsAsync(staffUser, spoofedFilter);

        // Service must override and lock CentreId to 1
        Assert.Equal(1, result.Filter.CentreId);
        Assert.All(result.CentreReports, c => Assert.Equal(1, c.CentreId));
        Assert.All(result.BeneficiaryReports.Items, b => Assert.Equal("Navchetan De-Addiction & Rehabilitation Centre", b.CentreName));
    }

    [Fact]
    public async Task DateFilters_RespectStartAndEndBoundaries()
    {
        var context = CreateInMemoryDbContext("TestDb_DateBoundaries");
        var (adminUser, _) = CreateTestUsers();

        var authMock = new MockCentreAuthService();
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        // Query future date where no beneficiaries were registered
        var futureFilter = new ReportsFilterViewModel
        {
            StartDate = DateTime.Today.AddYears(1),
            EndDate = DateTime.Today.AddYears(2)
        };

        var result = await reportService.GetReportsAsync(adminUser, futureFilter);

        Assert.Equal(0, result.Metrics.TotalRegisteredBeneficiaries);
        Assert.Empty(result.BeneficiaryReports.Items);
    }

    [Fact]
    public async Task CsvExport_EscapesFormulasAndSpecialCharacters()
    {
        var context = CreateInMemoryDbContext("TestDb_CsvProtection");
        var (adminUser, _) = CreateTestUsers();

        // Inject beneficiary with Excel formula injection payload
        var maliciousBen = new Beneficiary
        {
            BeneficiaryId = 8881,
            BeneficiaryCode = "=cmd|'/C calc'!A0",
            CentreId = 1,
            FullName = "+2+5;DROP TABLE Beneficiaries",
            RegistrationDate = DateTime.Today,
            Status = "Active"
        };
        context.Beneficiaries.Add(maliciousBen);
        await context.SaveChangesAsync();

        var authMock = new MockCentreAuthService();
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        var filter = new ReportsFilterViewModel { CentreId = 1 };
        var csvBytes = await reportService.ExportBeneficiariesCsvAsync(adminUser, filter);
        var csvText = System.Text.Encoding.UTF8.GetString(csvBytes);

        // Verify formula injection payload is prefixed with single quote
        Assert.Contains("'=cmd|'/C calc'!A0", csvText);
        Assert.Contains("'+2+5;DROP TABLE Beneficiaries", csvText);
    }

    [Fact]
    public async Task StateTotals_MatchSumOfCentres()
    {
        var context = CreateInMemoryDbContext("TestDb_StateSumCheck");
        var (adminUser, _) = CreateTestUsers();

        var authMock = new MockCentreAuthService();
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        var filter = new ReportsFilterViewModel();
        var result = await reportService.GetReportsAsync(adminUser, filter);

        var sumCentresBen = result.CentreReports.Sum(c => c.TotalBeneficiaries);
        var sumStatesBen = result.StateReports.Sum(s => s.TotalBeneficiaries);

        Assert.Equal(sumCentresBen, sumStatesBen);
        Assert.Equal(result.Metrics.TotalRegisteredBeneficiaries, sumCentresBen);
    }

    [Fact]
    public async Task EmptyResults_HandledGracefully_WhenNoRecordsMatch()
    {
        var context = CreateInMemoryDbContext("TestDb_EmptyHandling");
        var (adminUser, _) = CreateTestUsers();

        var authMock = new MockCentreAuthService();
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        var filter = new ReportsFilterViewModel { District = "NonExistentDistrictXYZ" };
        var result = await reportService.GetReportsAsync(adminUser, filter);

        Assert.Equal(0, result.Metrics.TotalRegisteredBeneficiaries);
        Assert.Equal(0, result.Metrics.ActiveBeneficiaries);
        Assert.Empty(result.CentreReports);
        Assert.Empty(result.BeneficiaryReports.Items);
    }

    [Fact]
    public void StoredProcedureDefinition_UsesTypedParametersAndPreventsSqlInjection()
    {
        var sqlPath = Path.Combine("..", "..", "..", "..", "Data", "CompleteDatabasePart7.sql");
        Assert.True(File.Exists(sqlPath), $"Expected SQL file at {sqlPath}");

        var sqlContent = File.ReadAllText(sqlPath);

        // Verify Stored Procedure existence and typed parameters
        Assert.Contains("CREATE OR ALTER PROCEDURE usp_GetCentreServiceSummary", sqlContent);
        Assert.Contains("@CentreId INT = NULL", sqlContent);
        Assert.Contains("@StateId INT = NULL", sqlContent);
        Assert.Contains("@StartDate DATE = NULL", sqlContent);
        Assert.Contains("@EndDate DATE = NULL", sqlContent);

        // Verify stored procedure body uses parameterized SQL rather than dynamic SQL string concatenation
        var spStartIndex = sqlContent.IndexOf("CREATE OR ALTER PROCEDURE usp_GetCentreServiceSummary", StringComparison.Ordinal);
        Assert.True(spStartIndex > 0, "Stored procedure not found in SQL script");
        var spBody = sqlContent.Substring(spStartIndex);

        Assert.Contains("(@StartDate IS NULL OR RegistrationDate >= @StartDate)", spBody);
        Assert.Contains("(@EndDate IS NULL OR RegistrationDate <= @EndDate)", spBody);
        Assert.DoesNotContain("sp_executesql", spBody);
        Assert.DoesNotContain("EXEC(", spBody);
    }

    [Fact]
    public void SqlViews_UseIsolatedAggregationsToPreventCartesianExplosion()
    {
        var sqlPath = Path.Combine("..", "..", "..", "..", "Data", "CompleteDatabasePart7.sql");
        var sqlContent = File.ReadAllText(sqlPath);

        Assert.Contains("CREATE OR ALTER VIEW vw_CentreServiceSummary", sqlContent);
        Assert.Contains("CREATE OR ALTER VIEW vw_StateBeneficiarySummary", sqlContent);

        // Verify Common Table Expression / Subquery isolation
        Assert.Contains("WITH BenAgg AS", sqlContent);
        Assert.Contains("CounsellingAgg AS", sqlContent);
        Assert.Contains("InterventionAgg AS", sqlContent);
        Assert.Contains("FollowUpAgg AS", sqlContent);
    }

    [Fact]
    public async Task CsvExport_OmitsConfidentialClinicalNotes()
    {
        var context = CreateInMemoryDbContext("TestDb_ConfidentialNotesOmitted");
        var (adminUser, _) = CreateTestUsers();

        // Add a session with a sensitive note
        var session = new CounsellingSession
        {
            SessionId = 7771,
            BeneficiaryId = 1,
            SessionDate = DateTime.Today,
            SessionType = "Individual",
            SessionStatus = "Completed",
            Notes = "CONFIDENTIAL_PATIENT_PSYCHOLOGICAL_TRAUMA_RECORD_XYZ"
        };
        context.CounsellingSessions.Add(session);
        await context.SaveChangesAsync();

        var authMock = new MockCentreAuthService();
        var reportService = new ReportService(context, authMock, NullLogger<ReportService>.Instance);

        var filter = new ReportsFilterViewModel();
        var summaryCsv = await reportService.ExportSummaryCsvAsync(adminUser, filter);
        var benCsv = await reportService.ExportBeneficiariesCsvAsync(adminUser, filter);

        var summaryText = System.Text.Encoding.UTF8.GetString(summaryCsv);
        var benText = System.Text.Encoding.UTF8.GetString(benCsv);

        Assert.DoesNotContain("CONFIDENTIAL_PATIENT_PSYCHOLOGICAL_TRAUMA_RECORD_XYZ", summaryText);
        Assert.DoesNotContain("CONFIDENTIAL_PATIENT_PSYCHOLOGICAL_TRAUMA_RECORD_XYZ", benText);
    }
}

public class MockCentreAuthService : ICentreAuthorizationService
{
    public bool MockIsAdmin { get; set; } = true;
    public int? MockUserCentreId { get; set; } = null;

    public Task<bool> IsAdminAsync(ClaimsPrincipal user) => Task.FromResult(MockIsAdmin);
    public Task<int?> GetUserCentreIdAsync(ClaimsPrincipal user) => Task.FromResult(MockUserCentreId);
    public Task<bool> CanAccessCentreAsync(ClaimsPrincipal user, int centreId)
    {
        if (MockIsAdmin) return Task.FromResult(true);
        return Task.FromResult(MockUserCentreId.HasValue && MockUserCentreId.Value == centreId);
    }
    public Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal principal) => Task.FromResult<ApplicationUser?>(null);
    public Task<bool> CanAccessBeneficiaryAsync(ClaimsPrincipal principal, int beneficiaryId) => Task.FromResult(true);
}
