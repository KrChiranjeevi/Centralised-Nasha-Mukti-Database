using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Reports & Analytics Controller — Delivers multi-dimensional reports, Chart.js datasets,
/// and sanitized CSV downloads with strict Centre-level scoping.
/// </summary>
[Authorize]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    // GET: /Reports
    public async Task<IActionResult> Index(ReportsFilterViewModel filter, int page = 1)
    {
        ViewData["Title"] = "Reports & Analytics";
        var vm = await _reportService.GetReportsAsync(User, filter, page, pageSize: 15);
        return View(vm);
    }

    // GET: /Reports/ExportSummary
    [HttpGet]
    public async Task<IActionResult> ExportSummary(ReportsFilterViewModel filter)
    {
        var bytes = await _reportService.ExportSummaryCsvAsync(User, filter);
        var filename = $"NashaMukti_CentreSummary_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        _logger.LogInformation("Generated Summary CSV export by user {User}", User.Identity?.Name);
        return File(bytes, "text/csv; charset=utf-8", filename);
    }

    // GET: /Reports/ExportBeneficiaries
    [HttpGet]
    public async Task<IActionResult> ExportBeneficiaries(ReportsFilterViewModel filter)
    {
        var bytes = await _reportService.ExportBeneficiariesCsvAsync(User, filter);
        var filename = $"NashaMukti_BeneficiariesDossier_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        _logger.LogInformation("Generated Beneficiaries CSV export by user {User}", User.Identity?.Name);
        return File(bytes, "text/csv; charset=utf-8", filename);
    }
}
