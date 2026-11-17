using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NashaMuktiDB.ViewModels;

/// <summary>
/// Filter model for reports and analytics dashboards.
/// Enforces CentreStaff boundary in the service layer.
/// </summary>
public class ReportsFilterViewModel
{
    [Display(Name = "State")]
    public int? StateId { get; set; }

    [Display(Name = "District")]
    public string? District { get; set; }

    [Display(Name = "Centre")]
    public int? CentreId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }

    public string ActiveTab { get; set; } = "overview"; // overview, centres, states, beneficiaries

    // Reference dropdown options
    public List<SelectListItem> StatesList { get; set; } = new();
    public List<SelectListItem> CentresList { get; set; } = new();
    public List<SelectListItem> DistrictsList { get; set; } = new();

    // Security & User context
    public bool IsAdmin { get; set; }
    public int? UserCentreId { get; set; }
    public string? UserCentreName { get; set; }
}

/// <summary>
/// Metric cards representing core database KPIs with clear counting rules.
/// </summary>
public class DashboardMetricsViewModel
{
    public int TotalRegisteredBeneficiaries { get; set; }
    public int ActiveBeneficiaries { get; set; }
    public int TotalRegisteredCentres { get; set; }
    public int ActiveCentres { get; set; }
    public int TotalCounsellingSessions { get; set; }
    public int TotalRecordedInterventions { get; set; }
    public int PendingFollowUps { get; set; }
    public int OverdueFollowUps { get; set; }
    public int NewRegistrationsInPeriod { get; set; }
    public string PeriodDescription { get; set; } = "All Time";
}

/// <summary>
/// Aggregated centre report row.
/// </summary>
public class CentreReportItemViewModel
{
    public int CentreId { get; set; }
    public string CentreName { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalBeneficiaries { get; set; }
    public int ActiveBeneficiaries { get; set; }
    public int TotalCounsellingSessions { get; set; }
    public int CompletedCounsellingSessions { get; set; }
    public int TotalInterventions { get; set; }
    public int TotalFollowUps { get; set; }
    public int PendingFollowUps { get; set; }
    public int OverdueFollowUps { get; set; }
}

/// <summary>
/// Aggregated state-level summary report row.
/// </summary>
public class StateReportItemViewModel
{
    public int StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalCentres { get; set; }
    public int ActiveCentres { get; set; }
    public int TotalBeneficiaries { get; set; }
    public int ActiveBeneficiaries { get; set; }
    public int TotalCounsellingSessions { get; set; }
    public int TotalInterventions { get; set; }
    public double BeneficiaryPercentage { get; set; }
}

/// <summary>
/// Restricted beneficiary record summary (strictly without confidential notes).
/// </summary>
public class BeneficiaryReportItemViewModel
{
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CounsellingCount { get; set; }
    public int InterventionCount { get; set; }
    public int PendingFollowUps { get; set; }
    public int CompletedFollowUps { get; set; }
}

/// <summary>
/// Monthly temporal trend item for Chart.js.
/// </summary>
public class MonthlyTrendItemViewModel
{
    public string MonthLabel { get; set; } = string.Empty;
    public int RegistrationCount { get; set; }
    public int CounsellingCount { get; set; }
    public int InterventionCount { get; set; }
}

/// <summary>
/// Categorical breakdown item for Chart.js doughnut / bar charts.
/// </summary>
public class CategoryDistributionItemViewModel
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Follow-up status breakdown.
/// </summary>
public class FollowUpStatusDistributionViewModel
{
    public int PendingCount { get; set; }
    public int CompletedCount { get; set; }
    public int MissedCount { get; set; }
    public int CancelledCount { get; set; }
    public int OverdueCount { get; set; }
}

/// <summary>
/// Complete view model for the Reports & Analytics view.
/// </summary>
public class ReportsIndexViewModel
{
    public ReportsFilterViewModel Filter { get; set; } = new();
    public DashboardMetricsViewModel Metrics { get; set; } = new();
    public List<CentreReportItemViewModel> CentreReports { get; set; } = new();
    public List<StateReportItemViewModel> StateReports { get; set; } = new();
    public PaginatedList<BeneficiaryReportItemViewModel> BeneficiaryReports { get; set; } = null!;
    public List<MonthlyTrendItemViewModel> MonthlyTrends { get; set; } = new();
    public List<CategoryDistributionItemViewModel> InterventionCategories { get; set; } = new();
    public List<CategoryDistributionItemViewModel> CounsellingTypes { get; set; } = new();
    public FollowUpStatusDistributionViewModel FollowUpStatusDistribution { get; set; } = new();

    // Data Source Notice (e.g. indicates SQL View / Stored Procedure usage)
    public string DataSourceNotice { get; set; } = "Queried live from SQL Server via vw_CentreServiceSummary & vw_StateBeneficiarySummary.";
}

