using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        ApplicationDbContext context,
        ICentreAuthorizationService authService,
        ILogger<ReportService> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    public async Task<ReportsFilterViewModel> PopulateFilterOptionsAsync(
        ClaimsPrincipal user,
        ReportsFilterViewModel filter)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        filter.IsAdmin = isAdmin;
        filter.UserCentreId = userCentreId;

        if (!isAdmin && userCentreId.HasValue)
        {
            // Lock filters to user's assigned facility
            filter.CentreId = userCentreId.Value;
            var centre = await _context.Centres
                .Include(c => c.State)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CentreId == userCentreId.Value);

            filter.UserCentreName = centre?.CentreName;
            filter.StateId = centre?.StateId;
            filter.District = centre?.District;

            filter.CentresList = new List<SelectListItem>
            {
                new SelectListItem { Value = userCentreId.Value.ToString(), Text = centre?.CentreName ?? "Assigned Centre", Selected = true }
            };

            filter.StatesList = centre?.State != null
                ? new List<SelectListItem> { new SelectListItem { Value = centre.State.StateId.ToString(), Text = centre.State.StateName, Selected = true } }
                : new List<SelectListItem>();

            filter.DistrictsList = !string.IsNullOrEmpty(centre?.District)
                ? new List<SelectListItem> { new SelectListItem { Value = centre.District, Text = centre.District, Selected = true } }
                : new List<SelectListItem>();

            return filter;
        }

        // Global Administrator dropdown options
        filter.StatesList = await _context.States
            .Where(s => s.IsActive)
            .OrderBy(s => s.StateName)
            .Select(s => new SelectListItem
            {
                Value = s.StateId.ToString(),
                Text = s.StateName,
                Selected = filter.StateId == s.StateId
            })
            .ToListAsync();

        filter.CentresList = await _context.Centres
            .Where(c => c.IsActive && (!filter.StateId.HasValue || c.StateId == filter.StateId.Value))
            .OrderBy(c => c.CentreName)
            .Select(c => new SelectListItem
            {
                Value = c.CentreId.ToString(),
                Text = $"{c.CentreName} ({c.District})",
                Selected = filter.CentreId == c.CentreId
            })
            .ToListAsync();

        filter.DistrictsList = await _context.Centres
            .Where(c => c.IsActive && (!filter.StateId.HasValue || c.StateId == filter.StateId.Value))
            .Select(c => c.District)
            .Distinct()
            .OrderBy(d => d)
            .Select(d => new SelectListItem
            {
                Value = d,
                Text = d,
                Selected = filter.District == d
            })
            .ToListAsync();

        return filter;
    }

    public async Task<ReportsIndexViewModel> GetReportsAsync(
        ClaimsPrincipal user,
        ReportsFilterViewModel filter,
        int page = 1,
        int pageSize = 15)
    {
        // 1. Enforce Role & Centre Scoping
        filter = await PopulateFilterOptionsAsync(user, filter);

        var startDate = filter.StartDate?.Date;
        var endDate = filter.EndDate?.Date;

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            // Swap if user inverted dates
            (startDate, endDate) = (endDate, startDate);
            filter.StartDate = startDate;
            filter.EndDate = endDate;
        }

        // 2. Base Queries with strict Centre scoping
        var centresQuery = _context.Centres.Include(c => c.State).AsNoTracking().AsQueryable();
        var beneficiariesQuery = _context.Beneficiaries.Include(b => b.Centre).ThenInclude(c => c!.State).AsNoTracking().AsQueryable();
        var sessionsQuery = _context.CounsellingSessions.Include(s => s.Beneficiary).ThenInclude(b => b!.Centre).AsNoTracking().AsQueryable();
        var interventionsQuery = _context.Interventions.Include(i => i.Beneficiary).ThenInclude(b => b!.Centre).AsNoTracking().AsQueryable();
        var followUpsQuery = _context.FollowUps.Include(f => f.Beneficiary).ThenInclude(b => b!.Centre).AsNoTracking().AsQueryable();

        // Apply Centre/State/District filters
        if (filter.CentreId.HasValue)
        {
            centresQuery = centresQuery.Where(c => c.CentreId == filter.CentreId.Value);
            beneficiariesQuery = beneficiariesQuery.Where(b => b.CentreId == filter.CentreId.Value);
            sessionsQuery = sessionsQuery.Where(s => s.Beneficiary != null && s.Beneficiary.CentreId == filter.CentreId.Value);
            interventionsQuery = interventionsQuery.Where(i => i.Beneficiary != null && i.Beneficiary.CentreId == filter.CentreId.Value);
            followUpsQuery = followUpsQuery.Where(f => f.Beneficiary != null && f.Beneficiary.CentreId == filter.CentreId.Value);
        }
        else
        {
            if (filter.StateId.HasValue)
            {
                centresQuery = centresQuery.Where(c => c.StateId == filter.StateId.Value);
                beneficiariesQuery = beneficiariesQuery.Where(b => b.Centre != null && b.Centre.StateId == filter.StateId.Value);
                sessionsQuery = sessionsQuery.Where(s => s.Beneficiary != null && s.Beneficiary.Centre != null && s.Beneficiary.Centre.StateId == filter.StateId.Value);
                interventionsQuery = interventionsQuery.Where(i => i.Beneficiary != null && i.Beneficiary.Centre != null && i.Beneficiary.Centre.StateId == filter.StateId.Value);
                followUpsQuery = followUpsQuery.Where(f => f.Beneficiary != null && f.Beneficiary.Centre != null && f.Beneficiary.Centre.StateId == filter.StateId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.District))
            {
                centresQuery = centresQuery.Where(c => c.District == filter.District);
                beneficiariesQuery = beneficiariesQuery.Where(b => b.Centre != null && b.Centre.District == filter.District);
                sessionsQuery = sessionsQuery.Where(s => s.Beneficiary != null && s.Beneficiary.Centre != null && s.Beneficiary.Centre.District == filter.District);
                interventionsQuery = interventionsQuery.Where(i => i.Beneficiary != null && i.Beneficiary.Centre != null && i.Beneficiary.Centre.District == filter.District);
                followUpsQuery = followUpsQuery.Where(f => f.Beneficiary != null && f.Beneficiary.Centre != null && f.Beneficiary.Centre.District == filter.District);
            }
        }

        // Apply Date Boundaries
        if (startDate.HasValue)
        {
            beneficiariesQuery = beneficiariesQuery.Where(b => b.RegistrationDate >= startDate.Value);
            sessionsQuery = sessionsQuery.Where(s => s.SessionDate >= startDate.Value);
            interventionsQuery = interventionsQuery.Where(i => i.ServiceDate >= startDate.Value);
            followUpsQuery = followUpsQuery.Where(f => f.ScheduledDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            beneficiariesQuery = beneficiariesQuery.Where(b => b.RegistrationDate <= endDate.Value);
            sessionsQuery = sessionsQuery.Where(s => s.SessionDate <= endDate.Value);
            interventionsQuery = interventionsQuery.Where(i => i.ServiceDate <= endDate.Value);
            followUpsQuery = followUpsQuery.Where(f => f.ScheduledDate <= endDate.Value);
        }

        // 3. Compute High-Level Metrics (Zero Row Multiplication)
        var totalBeneficiaries = await beneficiariesQuery.CountAsync();
        var activeBeneficiaries = await beneficiariesQuery.CountAsync(b => b.Status == "Active");
        var totalCentres = await centresQuery.CountAsync();
        var activeCentres = await centresQuery.CountAsync(c => c.IsActive);
        var totalCounselling = await sessionsQuery.CountAsync();
        var totalInterventions = await interventionsQuery.CountAsync();
        var pendingFollowUps = await followUpsQuery.CountAsync(f => f.Status == "Pending");
        var today = DateTime.Today;
        var overdueFollowUps = await followUpsQuery.CountAsync(f => f.Status == "Pending" && f.ScheduledDate < today);

        string periodDesc = "All Records";
        if (startDate.HasValue && endDate.HasValue)
            periodDesc = $"{startDate.Value:dd MMM yyyy} to {endDate.Value:dd MMM yyyy}";
        else if (startDate.HasValue)
            periodDesc = $"From {startDate.Value:dd MMM yyyy}";
        else if (endDate.HasValue)
            periodDesc = $"Up to {endDate.Value:dd MMM yyyy}";

        var metrics = new DashboardMetricsViewModel
        {
            TotalRegisteredBeneficiaries = totalBeneficiaries,
            ActiveBeneficiaries = activeBeneficiaries,
            TotalRegisteredCentres = totalCentres,
            ActiveCentres = activeCentres,
            TotalCounsellingSessions = totalCounselling,
            TotalRecordedInterventions = totalInterventions,
            PendingFollowUps = pendingFollowUps,
            OverdueFollowUps = overdueFollowUps,
            NewRegistrationsInPeriod = totalBeneficiaries,
            PeriodDescription = periodDesc
        };

        // 4. Centre-wise Summary (Aggregated independently to prevent Cartesian explosion)
        var centreIds = await centresQuery.Select(c => c.CentreId).ToListAsync();

        var benByCentre = await _context.Beneficiaries
            .Where(b => centreIds.Contains(b.CentreId))
            .Where(b => (!startDate.HasValue || b.RegistrationDate >= startDate.Value) && (!endDate.HasValue || b.RegistrationDate <= endDate.Value))
            .GroupBy(b => b.CentreId)
            .Select(g => new
            {
                CentreId = g.Key,
                Total = g.Count(),
                Active = g.Count(b => b.Status == "Active")
            })
            .ToDictionaryAsync(x => x.CentreId);

        var sessByCentre = await _context.CounsellingSessions
            .Where(s => s.Beneficiary != null && centreIds.Contains(s.Beneficiary.CentreId))
            .Where(s => (!startDate.HasValue || s.SessionDate >= startDate.Value) && (!endDate.HasValue || s.SessionDate <= endDate.Value))
            .GroupBy(s => s.Beneficiary!.CentreId)
            .Select(g => new
            {
                CentreId = g.Key,
                Total = g.Count(),
                Completed = g.Count(s => s.SessionStatus == "Completed")
            })
            .ToDictionaryAsync(x => x.CentreId);

        var intByCentre = await _context.Interventions
            .Where(i => i.Beneficiary != null && centreIds.Contains(i.Beneficiary.CentreId))
            .Where(i => (!startDate.HasValue || i.ServiceDate >= startDate.Value) && (!endDate.HasValue || i.ServiceDate <= endDate.Value))
            .GroupBy(i => i.Beneficiary!.CentreId)
            .Select(g => new { CentreId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.CentreId);

        var fuByCentre = await _context.FollowUps
            .Where(f => f.Beneficiary != null && centreIds.Contains(f.Beneficiary.CentreId))
            .Where(f => (!startDate.HasValue || f.ScheduledDate >= startDate.Value) && (!endDate.HasValue || f.ScheduledDate <= endDate.Value))
            .GroupBy(f => f.Beneficiary!.CentreId)
            .Select(g => new
            {
                CentreId = g.Key,
                Total = g.Count(),
                Pending = g.Count(f => f.Status == "Pending"),
                Overdue = g.Count(f => f.Status == "Pending" && f.ScheduledDate < today)
            })
            .ToDictionaryAsync(x => x.CentreId);

        var centreReports = await centresQuery
            .OrderBy(c => c.CentreName)
            .Select(c => new CentreReportItemViewModel
            {
                CentreId = c.CentreId,
                CentreName = c.CentreName,
                StateName = c.State != null ? c.State.StateName : string.Empty,
                District = c.District,
                IsActive = c.IsActive,
                TotalBeneficiaries = benByCentre.ContainsKey(c.CentreId) ? benByCentre[c.CentreId].Total : 0,
                ActiveBeneficiaries = benByCentre.ContainsKey(c.CentreId) ? benByCentre[c.CentreId].Active : 0,
                TotalCounsellingSessions = sessByCentre.ContainsKey(c.CentreId) ? sessByCentre[c.CentreId].Total : 0,
                CompletedCounsellingSessions = sessByCentre.ContainsKey(c.CentreId) ? sessByCentre[c.CentreId].Completed : 0,
                TotalInterventions = intByCentre.ContainsKey(c.CentreId) ? intByCentre[c.CentreId].Total : 0,
                TotalFollowUps = fuByCentre.ContainsKey(c.CentreId) ? fuByCentre[c.CentreId].Total : 0,
                PendingFollowUps = fuByCentre.ContainsKey(c.CentreId) ? fuByCentre[c.CentreId].Pending : 0,
                OverdueFollowUps = fuByCentre.ContainsKey(c.CentreId) ? fuByCentre[c.CentreId].Overdue : 0
            })
            .ToListAsync();

        // 5. State-wise Summary
        var statesList = await _context.States.Where(s => s.IsActive).OrderBy(s => s.StateName).ToListAsync();
        var stateReports = new List<StateReportItemViewModel>();

        foreach (var s in statesList)
        {
            var matchingCentres = centreReports.Where(cr => cr.StateName == s.StateName).ToList();
            var stateBen = matchingCentres.Sum(cr => cr.TotalBeneficiaries);
            var pct = totalBeneficiaries > 0 ? (stateBen / (double)totalBeneficiaries) * 100.0 : 0.0;

            stateReports.Add(new StateReportItemViewModel
            {
                StateId = s.StateId,
                StateName = s.StateName,
                IsActive = s.IsActive,
                TotalCentres = matchingCentres.Count,
                ActiveCentres = matchingCentres.Count(c => c.IsActive),
                TotalBeneficiaries = stateBen,
                ActiveBeneficiaries = matchingCentres.Sum(cr => cr.ActiveBeneficiaries),
                TotalCounsellingSessions = matchingCentres.Sum(cr => cr.TotalCounsellingSessions),
                TotalInterventions = matchingCentres.Sum(cr => cr.TotalInterventions),
                BeneficiaryPercentage = Math.Round(pct, 1)
            });
        }

        // 6. Beneficiary Clinical Dossier Table (Paginated, No Confidential Notes)
        var benProjected = beneficiariesQuery
            .OrderByDescending(b => b.RegistrationDate)
            .ThenByDescending(b => b.BeneficiaryId)
            .Select(b => new BeneficiaryReportItemViewModel
            {
                BeneficiaryId = b.BeneficiaryId,
                BeneficiaryCode = b.BeneficiaryCode,
                FullName = b.FullName,
                CentreName = b.Centre != null ? b.Centre.CentreName : "N/A",
                District = b.Centre != null ? b.Centre.District : "N/A",
                StateName = b.Centre != null && b.Centre.State != null ? b.Centre.State.StateName : "N/A",
                RegistrationDate = b.RegistrationDate,
                Status = b.Status,
                CounsellingCount = b.CounsellingSessions.Count,
                InterventionCount = b.Interventions.Count,
                PendingFollowUps = b.FollowUps.Count(f => f.Status == "Pending"),
                CompletedFollowUps = b.FollowUps.Count(f => f.Status == "Completed")
            });

        var paginatedBeneficiaries = await PaginatedList<BeneficiaryReportItemViewModel>.CreateAsync(
            benProjected, page, pageSize);

        // 7. Monthly Temporal Trends (Last 12 Months or within range)
        var now = DateTime.UtcNow;
        var months = Enumerable.Range(0, 12)
            .Select(i => now.AddMonths(-11 + i))
            .Select(d => new { Year = d.Year, Month = d.Month, Label = d.ToString("MMM yyyy") })
            .ToList();

        var monthlyTrends = new List<MonthlyTrendItemViewModel>();
        foreach (var m in months)
        {
            var regCount = await beneficiariesQuery.CountAsync(b => b.RegistrationDate.Year == m.Year && b.RegistrationDate.Month == m.Month);
            var sesCount = await sessionsQuery.CountAsync(s => s.SessionDate.Year == m.Year && s.SessionDate.Month == m.Month);
            var intCount = await interventionsQuery.CountAsync(i => i.ServiceDate.Year == m.Year && i.ServiceDate.Month == m.Month);

            monthlyTrends.Add(new MonthlyTrendItemViewModel
            {
                MonthLabel = m.Label,
                RegistrationCount = regCount,
                CounsellingCount = sesCount,
                InterventionCount = intCount
            });
        }

        // 8. Categorical Distributions
        var intCategories = await interventionsQuery
            .GroupBy(i => i.InterventionType)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalIntCount = intCategories.Sum(c => c.Count);
        var interventionDist = intCategories.Select(c => new CategoryDistributionItemViewModel
        {
            Category = c.Category,
            Count = c.Count,
            Percentage = totalIntCount > 0 ? Math.Round((c.Count / (double)totalIntCount) * 100.0, 1) : 0
        }).OrderByDescending(c => c.Count).ToList();

        var sesTypes = await sessionsQuery
            .GroupBy(s => s.SessionType)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalSesCount = sesTypes.Sum(c => c.Count);
        var counsellingDist = sesTypes.Select(c => new CategoryDistributionItemViewModel
        {
            Category = c.Category,
            Count = c.Count,
            Percentage = totalSesCount > 0 ? Math.Round((c.Count / (double)totalSesCount) * 100.0, 1) : 0
        }).OrderByDescending(c => c.Count).ToList();

        // Follow-up status distribution
        var fuPending = await followUpsQuery.CountAsync(f => f.Status == "Pending" && f.ScheduledDate >= today);
        var fuOverdue = await followUpsQuery.CountAsync(f => f.Status == "Pending" && f.ScheduledDate < today);
        var fuCompleted = await followUpsQuery.CountAsync(f => f.Status == "Completed");
        var fuMissed = await followUpsQuery.CountAsync(f => f.Status == "Missed");
        var fuCancelled = await followUpsQuery.CountAsync(f => f.Status == "Cancelled");

        var fuDistribution = new FollowUpStatusDistributionViewModel
        {
            PendingCount = fuPending,
            OverdueCount = fuOverdue,
            CompletedCount = fuCompleted,
            MissedCount = fuMissed,
            CancelledCount = fuCancelled
        };

        return new ReportsIndexViewModel
        {
            Filter = filter,
            Metrics = metrics,
            CentreReports = centreReports,
            StateReports = stateReports,
            BeneficiaryReports = paginatedBeneficiaries,
            MonthlyTrends = monthlyTrends,
            InterventionCategories = interventionDist,
            CounsellingTypes = counsellingDist,
            FollowUpStatusDistribution = fuDistribution,
            DataSourceNotice = "Derived live from SQL Server Code-First entities, aggregated with zero row multiplication."
        };
    }

    public async Task<byte[]> ExportSummaryCsvAsync(ClaimsPrincipal user, ReportsFilterViewModel filter)
    {
        var model = await GetReportsAsync(user, filter, page: 1, pageSize: 1);

        var sb = new StringBuilder();
        // BOM for Excel UTF-8 compatibility
        sb.Append('\uFEFF');

        // Header / Metadata
        sb.AppendLine("# NASHA MUKTI CENTRALISED DATABASE - CENTRE SUMMARY REPORT");
        sb.AppendLine($"# Generated On: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"# Scope: {(filter.IsAdmin ? "National / Multi-Centre" : $"Restricted to Facility: {filter.UserCentreName}")}");
        sb.AppendLine($"# Period: {model.Metrics.PeriodDescription}");
        sb.AppendLine("#");

        // Column Headers
        sb.AppendLine("Centre Name,State,District,Status,Total Beneficiaries,Active Beneficiaries,Counselling Sessions,Completed Sessions,Interventions,Total Follow-ups,Pending Follow-ups,Overdue Follow-ups");

        foreach (var c in model.CentreReports)
        {
            sb.Append(EscapeCsv(c.CentreName)).Append(',');
            sb.Append(EscapeCsv(c.StateName)).Append(',');
            sb.Append(EscapeCsv(c.District)).Append(',');
            sb.Append(c.IsActive ? "Active" : "Inactive").Append(',');
            sb.Append(c.TotalBeneficiaries).Append(',');
            sb.Append(c.ActiveBeneficiaries).Append(',');
            sb.Append(c.TotalCounsellingSessions).Append(',');
            sb.Append(c.CompletedCounsellingSessions).Append(',');
            sb.Append(c.TotalInterventions).Append(',');
            sb.Append(c.TotalFollowUps).Append(',');
            sb.Append(c.PendingFollowUps).Append(',');
            sb.Append(c.OverdueFollowUps);
            sb.AppendLine();
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportBeneficiariesCsvAsync(ClaimsPrincipal user, ReportsFilterViewModel filter)
    {
        var model = await GetReportsAsync(user, filter, page: 1, pageSize: 5000);

        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        sb.AppendLine("# NASHA MUKTI CENTRALISED DATABASE - BENEFICIARY CLINICAL DOSSIER REPORT");
        sb.AppendLine($"# Generated On: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"# Scope: {(filter.IsAdmin ? "National Administrator" : $"Facility: {filter.UserCentreName}")}");
        sb.AppendLine($"# Period: {model.Metrics.PeriodDescription}");
        sb.AppendLine("# Note: Confidential clinical and therapy notes are strictly omitted from this export.");
        sb.AppendLine("#");

        sb.AppendLine("Beneficiary ID,Full Name,Centre Name,District,State,Registration Date,Status,Counselling Sessions,Interventions,Pending Follow-ups,Completed Follow-ups");

        foreach (var b in model.BeneficiaryReports.Items)
        {
            sb.Append(EscapeCsv(b.BeneficiaryCode)).Append(',');
            sb.Append(EscapeCsv(b.FullName)).Append(',');
            sb.Append(EscapeCsv(b.CentreName)).Append(',');
            sb.Append(EscapeCsv(b.District)).Append(',');
            sb.Append(EscapeCsv(b.StateName)).Append(',');
            sb.Append(b.RegistrationDate.ToString("yyyy-MM-dd")).Append(',');
            sb.Append(EscapeCsv(b.Status)).Append(',');
            sb.Append(b.CounsellingCount).Append(',');
            sb.Append(b.InterventionCount).Append(',');
            sb.Append(b.PendingFollowUps).Append(',');
            sb.Append(b.CompletedFollowUps);
            sb.AppendLine();
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// Escapes CSV field value and protects against CSV formula injection (CWE-1236).
    /// </summary>
    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        var clean = value;

        // Formula injection mitigation: If string starts with =, +, -, @, \t, \r, prepend a single quote
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

        // Escape double quotes
        if (clean.Contains('"') || clean.Contains(',') || clean.Contains('\n') || clean.Contains('\r'))
        {
            clean = "\"" + clean.Replace("\"", "\"\"") + "\"";
        }
        else
        {
            clean = "\"" + clean + "\"";
        }

        return clean;
    }
}

