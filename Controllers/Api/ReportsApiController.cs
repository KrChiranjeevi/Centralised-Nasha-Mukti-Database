using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers.Api;

[Route("api/v1/reports")]
[ApiController]
[Authorize]
public class ReportsApiController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsApiController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Retrieve core database KPI metrics (the 9 specified indicators).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<ReportSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] int? stateId,
        [FromQuery] int? centreId,
        [FromQuery] string? district,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var filter = new ReportsFilterViewModel
        {
            StateId = stateId,
            CentreId = centreId,
            District = district,
            StartDate = startDate,
            EndDate = endDate
        };

        var report = await _reportService.GetReportsAsync(User, filter, page: 1, pageSize: 1);

        var dto = new ReportSummaryDto
        {
            TotalRegisteredBeneficiaries = report.Metrics.TotalRegisteredBeneficiaries,
            ActiveBeneficiaries = report.Metrics.ActiveBeneficiaries,
            TotalRegisteredCentres = report.Metrics.TotalRegisteredCentres,
            ActiveCentres = report.Metrics.ActiveCentres,
            TotalCounsellingSessions = report.Metrics.TotalCounsellingSessions,
            TotalRecordedInterventions = report.Metrics.TotalRecordedInterventions,
            PendingFollowUps = report.Metrics.PendingFollowUps,
            OverdueFollowUps = report.Metrics.OverdueFollowUps,
            NewRegistrationsInPeriod = report.Metrics.NewRegistrationsInPeriod,
            PeriodDescription = report.Metrics.PeriodDescription
        };

        return Ok(ApiResponse<ReportSummaryDto>.Ok(dto));
    }

    /// <summary>
    /// Retrieve state-wise aggregated summaries.
    /// </summary>
    [HttpGet("state-wise")]
    [ProducesResponseType(typeof(ApiResponse<List<StateWiseReportDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStateWiseReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var filter = new ReportsFilterViewModel
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var report = await _reportService.GetReportsAsync(User, filter, page: 1, pageSize: 1);

        var dtoList = report.StateReports.Select(s => new StateWiseReportDto
        {
            StateId = s.StateId,
            StateName = s.StateName,
            TotalCentres = s.TotalCentres,
            ActiveCentres = s.ActiveCentres,
            TotalBeneficiaries = s.TotalBeneficiaries,
            ActiveBeneficiaries = s.ActiveBeneficiaries,
            TotalCounsellingSessions = s.TotalCounsellingSessions,
            TotalInterventions = s.TotalInterventions,
            NationalSharePercentage = s.BeneficiaryPercentage
        }).ToList();

        return Ok(ApiResponse<List<StateWiseReportDto>>.Ok(dtoList));
    }

    /// <summary>
    /// Retrieve centre-wise performance summaries (derived from SQL View vw_CentreServiceSummary).
    /// </summary>
    [HttpGet("centre-wise")]
    [ProducesResponseType(typeof(ApiResponse<List<CentreWiseReportDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCentreWiseReport(
        [FromQuery] int? stateId,
        [FromQuery] int? centreId,
        [FromQuery] string? district,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var filter = new ReportsFilterViewModel
        {
            StateId = stateId,
            CentreId = centreId,
            District = district,
            StartDate = startDate,
            EndDate = endDate
        };

        var report = await _reportService.GetReportsAsync(User, filter, page: 1, pageSize: 1);

        var dtoList = report.CentreReports.Select(c => new CentreWiseReportDto
        {
            CentreId = c.CentreId,
            CentreName = c.CentreName,
            StateName = c.StateName,
            District = c.District,
            IsActive = c.IsActive,
            TotalBeneficiaries = c.TotalBeneficiaries,
            ActiveBeneficiaries = c.ActiveBeneficiaries,
            TotalCounsellingSessions = c.TotalCounsellingSessions,
            CompletedCounsellingSessions = c.CompletedCounsellingSessions,
            TotalInterventions = c.TotalInterventions,
            TotalFollowUps = c.TotalFollowUps,
            PendingFollowUps = c.PendingFollowUps,
            OverdueFollowUps = c.OverdueFollowUps
        }).ToList();

        return Ok(ApiResponse<List<CentreWiseReportDto>>.Ok(dtoList));
    }
}

