namespace NashaMuktiDB.DTOs;

public class ReportSummaryDto
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
    public string PeriodDescription { get; set; } = string.Empty;
}

public class StateWiseReportDto
{
    public int StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int TotalCentres { get; set; }
    public int ActiveCentres { get; set; }
    public int TotalBeneficiaries { get; set; }
    public int ActiveBeneficiaries { get; set; }
    public int TotalCounsellingSessions { get; set; }
    public int TotalInterventions { get; set; }
    public double NationalSharePercentage { get; set; }
}

public class CentreWiseReportDto
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

