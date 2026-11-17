using System.ComponentModel.DataAnnotations.Schema;

namespace NashaMuktiDB.Models;

/// <summary>
/// Keyless entity mapped to the SQL Server view vw_CentreServiceSummary.
/// Provides aggregated performance statistics per centre without cartesian duplication.
/// </summary>
public class CentreServiceSummaryView
{
    public int CentreId { get; set; }
    public string CentreName { get; set; } = string.Empty;
    public int StateId { get; set; }
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
/// Keyless entity mapped to the SQL Server view vw_StateBeneficiarySummary.
/// Aggregates state-level counts of centres, beneficiaries, and clinical services.
/// </summary>
public class StateBeneficiarySummaryView
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
}

/// <summary>
/// Typed result model returned by the stored procedure usp_GetCentreServiceSummary.
/// </summary>
public class CentreServiceSummarySpResult
{
    public int CentreId { get; set; }
    public string CentreName { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int BeneficiaryCount { get; set; }
    public int ActiveBeneficiaryCount { get; set; }
    public int CounsellingCount { get; set; }
    public int InterventionCount { get; set; }
    public int PendingFollowUpCount { get; set; }
    public int OverdueFollowUpCount { get; set; }
}

