using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NashaMuktiDB.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingViewsAndProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Create or Alter View: vw_CentreServiceSummary ──────────────────
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW vw_CentreServiceSummary AS
WITH BenAgg AS (
    SELECT 
        CentreId,
        COUNT(1) AS TotalBeneficiaries,
        COUNT(CASE WHEN Status = 'Active' THEN 1 END) AS ActiveBeneficiaries
    FROM Beneficiaries
    GROUP BY CentreId
),
CounsellingAgg AS (
    SELECT 
        b.CentreId,
        COUNT(1) AS TotalCounsellingSessions,
        COUNT(CASE WHEN cs.SessionStatus = 'Completed' THEN 1 END) AS CompletedCounsellingSessions
    FROM CounsellingSessions cs
    INNER JOIN Beneficiaries b ON cs.BeneficiaryId = b.BeneficiaryId
    GROUP BY b.CentreId
),
InterventionAgg AS (
    SELECT 
        b.CentreId,
        COUNT(1) AS TotalInterventions
    FROM Interventions i
    INNER JOIN Beneficiaries b ON i.BeneficiaryId = b.BeneficiaryId
    GROUP BY b.CentreId
),
FollowUpAgg AS (
    SELECT 
        b.CentreId,
        COUNT(1) AS TotalFollowUps,
        COUNT(CASE WHEN fu.Status = 'Pending' THEN 1 END) AS PendingFollowUps,
        COUNT(CASE WHEN fu.Status = 'Pending' AND fu.ScheduledDate < CAST(GETUTCDATE() AS DATE) THEN 1 END) AS OverdueFollowUps
    FROM FollowUps fu
    INNER JOIN Beneficiaries b ON fu.BeneficiaryId = b.BeneficiaryId
    GROUP BY b.CentreId
)
SELECT 
    c.CentreId,
    c.CentreName,
    c.StateId,
    s.StateName,
    c.District,
    c.IsActive,
    ISNULL(ba.TotalBeneficiaries, 0) AS TotalBeneficiaries,
    ISNULL(ba.ActiveBeneficiaries, 0) AS ActiveBeneficiaries,
    ISNULL(ca.TotalCounsellingSessions, 0) AS TotalCounsellingSessions,
    ISNULL(ca.CompletedCounsellingSessions, 0) AS CompletedCounsellingSessions,
    ISNULL(ia.TotalInterventions, 0) AS TotalInterventions,
    ISNULL(fa.TotalFollowUps, 0) AS TotalFollowUps,
    ISNULL(fa.PendingFollowUps, 0) AS PendingFollowUps,
    ISNULL(fa.OverdueFollowUps, 0) AS OverdueFollowUps
FROM Centres c
INNER JOIN States s ON c.StateId = s.StateId
LEFT JOIN BenAgg ba ON c.CentreId = ba.CentreId
LEFT JOIN CounsellingAgg ca ON c.CentreId = ca.CentreId
LEFT JOIN InterventionAgg ia ON c.CentreId = ia.CentreId
LEFT JOIN FollowUpAgg fa ON c.CentreId = fa.CentreId;
");

            // ── 2. Create or Alter View: vw_StateBeneficiarySummary ───────────────
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW vw_StateBeneficiarySummary AS
WITH CentreAgg AS (
    SELECT 
        StateId,
        COUNT(1) AS TotalCentres,
        COUNT(CASE WHEN IsActive = 1 THEN 1 END) AS ActiveCentres
    FROM Centres
    GROUP BY StateId
),
BenAgg AS (
    SELECT 
        c.StateId,
        COUNT(1) AS TotalBeneficiaries,
        COUNT(CASE WHEN b.Status = 'Active' THEN 1 END) AS ActiveBeneficiaries
    FROM Beneficiaries b
    INNER JOIN Centres c ON b.CentreId = c.CentreId
    GROUP BY c.StateId
),
CounsellingAgg AS (
    SELECT 
        c.StateId,
        COUNT(1) AS TotalCounsellingSessions
    FROM CounsellingSessions cs
    INNER JOIN Beneficiaries b ON cs.BeneficiaryId = b.BeneficiaryId
    INNER JOIN Centres c ON b.CentreId = c.CentreId
    GROUP BY c.StateId
),
InterventionAgg AS (
    SELECT 
        c.StateId,
        COUNT(1) AS TotalInterventions
    FROM Interventions i
    INNER JOIN Beneficiaries b ON i.BeneficiaryId = b.BeneficiaryId
    INNER JOIN Centres c ON b.CentreId = c.CentreId
    GROUP BY c.StateId
)
SELECT 
    s.StateId,
    s.StateName,
    s.IsActive,
    ISNULL(ca.TotalCentres, 0) AS TotalCentres,
    ISNULL(ca.ActiveCentres, 0) AS ActiveCentres,
    ISNULL(ba.TotalBeneficiaries, 0) AS TotalBeneficiaries,
    ISNULL(ba.ActiveBeneficiaries, 0) AS ActiveBeneficiaries,
    ISNULL(csa.TotalCounsellingSessions, 0) AS TotalCounsellingSessions,
    ISNULL(ia.TotalInterventions, 0) AS TotalInterventions
FROM States s
LEFT JOIN CentreAgg ca ON s.StateId = ca.StateId
LEFT JOIN BenAgg ba ON s.StateId = ba.StateId
LEFT JOIN CounsellingAgg csa ON s.StateId = csa.StateId
LEFT JOIN InterventionAgg ia ON s.StateId = ia.StateId;
");

            // ── 3. Create or Alter Stored Procedure: usp_GetCentreServiceSummary ─
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE usp_GetCentreServiceSummary
    @CentreId INT = NULL,
    @StateId INT = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentDate DATE = CAST(GETUTCDATE() AS DATE);

    SELECT 
        c.CentreId,
        c.CentreName,
        s.StateName,
        c.District,
        c.IsActive,
        ISNULL(b.BeneficiaryCount, 0) AS BeneficiaryCount,
        ISNULL(b.ActiveBeneficiaryCount, 0) AS ActiveBeneficiaryCount,
        ISNULL(cs.CounsellingCount, 0) AS CounsellingCount,
        ISNULL(i.InterventionCount, 0) AS InterventionCount,
        ISNULL(fu.PendingFollowUpCount, 0) AS PendingFollowUpCount,
        ISNULL(fu.OverdueFollowUpCount, 0) AS OverdueFollowUpCount
    FROM Centres c
    INNER JOIN States s ON c.StateId = s.StateId
    LEFT JOIN (
        SELECT 
            CentreId,
            COUNT(1) AS BeneficiaryCount,
            COUNT(CASE WHEN Status = 'Active' THEN 1 END) AS ActiveBeneficiaryCount
        FROM Beneficiaries
        WHERE (@StartDate IS NULL OR RegistrationDate >= @StartDate)
          AND (@EndDate IS NULL OR RegistrationDate <= @EndDate)
        GROUP BY CentreId
    ) b ON c.CentreId = b.CentreId
    LEFT JOIN (
        SELECT 
            ben.CentreId,
            COUNT(1) AS CounsellingCount
        FROM CounsellingSessions sess
        INNER JOIN Beneficiaries ben ON sess.BeneficiaryId = ben.BeneficiaryId
        WHERE (@StartDate IS NULL OR sess.SessionDate >= @StartDate)
          AND (@EndDate IS NULL OR sess.SessionDate <= @EndDate)
        GROUP BY ben.CentreId
    ) cs ON c.CentreId = cs.CentreId
    LEFT JOIN (
        SELECT 
            ben.CentreId,
            COUNT(1) AS InterventionCount
        FROM Interventions itv
        INNER JOIN Beneficiaries ben ON itv.BeneficiaryId = ben.BeneficiaryId
        WHERE (@StartDate IS NULL OR itv.ServiceDate >= @StartDate)
          AND (@EndDate IS NULL OR itv.ServiceDate <= @EndDate)
        GROUP BY ben.CentreId
    ) i ON c.CentreId = i.CentreId
    LEFT JOIN (
        SELECT 
            ben.CentreId,
            COUNT(CASE WHEN Status = 'Pending' THEN 1 END) AS PendingFollowUpCount,
            COUNT(CASE WHEN Status = 'Pending' AND ScheduledDate < @CurrentDate THEN 1 END) AS OverdueFollowUpCount
        FROM FollowUps flw
        INNER JOIN Beneficiaries ben ON flw.BeneficiaryId = ben.BeneficiaryId
        WHERE (@StartDate IS NULL OR flw.ScheduledDate >= @StartDate)
          AND (@EndDate IS NULL OR flw.ScheduledDate <= @EndDate)
        GROUP BY ben.CentreId
    ) fu ON c.CentreId = fu.CentreId
    WHERE (@CentreId IS NULL OR c.CentreId = @CentreId)
      AND (@StateId IS NULL OR c.StateId = @StateId)
    ORDER BY c.CentreName;
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS usp_GetCentreServiceSummary;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_StateBeneficiarySummary;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_CentreServiceSummary;");
        }
    }
}
