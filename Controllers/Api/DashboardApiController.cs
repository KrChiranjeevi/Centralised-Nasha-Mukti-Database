using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers.Api;

/// <summary>
/// Dashboard API — returns stub data for Phase 1 UI testing.
/// Phase 7 will replace all stubs with real EF Core database queries.
/// </summary>
[Route("api/v1/dashboard")]
[ApiController]
public class DashboardApiController : ControllerBase
{
    // GET /api/v1/dashboard/stats
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        // Phase 7 TODO: replace with real DB query via IDashboardService
        return Ok(new
        {
            totalBeneficiaries = 0,
            activeCentres      = 0,
            totalCounsellings  = 0,
            totalInterventions = 0
        });
    }

    // GET /api/v1/dashboard/monthly-trend
    [HttpGet("monthly-trend")]
    public IActionResult GetMonthlyTrend()
    {
        // Phase 7 TODO: real query grouped by month
        var months = Enumerable.Range(0, 12)
            .Select(i => DateTime.Now.AddMonths(-11 + i))
            .Select(d => new { label = d.ToString("MMM yy"), count = 0 })
            .ToList();
        return Ok(months);
    }

    // GET /api/v1/dashboard/state-stats
    [HttpGet("state-stats")]
    public IActionResult GetStateStats()
    {
        return Ok(Array.Empty<object>());
    }

    // GET /api/v1/dashboard/centre-stats
    [HttpGet("centre-stats")]
    public IActionResult GetCentreStats()
    {
        return Ok(Array.Empty<object>());
    }

    // GET /api/v1/dashboard/recent-beneficiaries
    [HttpGet("recent-beneficiaries")]
    public IActionResult GetRecentBeneficiaries()
    {
        return Ok(Array.Empty<object>());
    }

    // GET /api/v1/dashboard/followup-stats
    [HttpGet("followup-stats")]
    public IActionResult GetFollowupStats()
    {
        return Ok(new { pending = 0, completed = 0, missed = 0, cancelled = 0 });
    }
}
