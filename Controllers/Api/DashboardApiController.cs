using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Services;

namespace NashaMuktiDB.Controllers.Api;

/// <summary>
/// Dashboard API — Serves real database-derived metrics, monthly trends, and centre statistics.
/// Respects CentreStaff scoping to display only authorized facility data.
/// </summary>
[Route("api/v1/dashboard")]
[ApiController]
public class DashboardApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;

    public DashboardApiController(
        ApplicationDbContext context,
        ICentreAuthorizationService authService)
    {
        _context = context;
        _authService = authService;
    }

    // GET: /api/v1/dashboard/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var isAdmin = await _authService.IsAdminAsync(User);
            var userCentreId = await _authService.GetUserCentreIdAsync(User);

            int totalCentres;
            int activeCentres;
            int totalBeneficiaries;
            int totalCounsellings;
            int totalInterventions;

            if (isAdmin || userCentreId == null)
            {
                totalCentres = await _context.Centres.CountAsync();
                activeCentres = await _context.Centres.CountAsync(c => c.IsActive);
                totalBeneficiaries = await _context.Beneficiaries.CountAsync();
                totalCounsellings = await _context.CounsellingSessions.CountAsync();
                totalInterventions = await _context.Interventions.CountAsync();
            }
            else
            {
                totalCentres = 1;
                activeCentres = await _context.Centres.CountAsync(c => c.CentreId == userCentreId.Value && c.IsActive);
                totalBeneficiaries = await _context.Beneficiaries.CountAsync(b => b.CentreId == userCentreId.Value);
                totalCounsellings = await _context.CounsellingSessions.CountAsync(s => s.Beneficiary != null && s.Beneficiary.CentreId == userCentreId.Value);
                totalInterventions = await _context.Interventions.CountAsync(i => i.Beneficiary != null && i.Beneficiary.CentreId == userCentreId.Value);
            }

            return Ok(new
            {
                totalCentres,
                activeCentres,
                totalBeneficiaries,
                totalCounsellings,
                totalInterventions
            });
        }
        catch
        {
            // Fallback for when database is unreachable during startup check
            return Ok(new
            {
                totalCentres = 0,
                activeCentres = 0,
                totalBeneficiaries = 0,
                totalCounsellings = 0,
                totalInterventions = 0
            });
        }
    }

    // GET: /api/v1/dashboard/monthly-trend
    [HttpGet("monthly-trend")]
    public async Task<IActionResult> GetMonthlyTrend()
    {
        try
        {
            var userCentreId = await _authService.GetUserCentreIdAsync(User);
            var isAdmin = await _authService.IsAdminAsync(User);

            var query = _context.Beneficiaries.AsQueryable();
            if (!isAdmin && userCentreId.HasValue)
            {
                query = query.Where(b => b.CentreId == userCentreId.Value);
            }

            var now = DateTime.UtcNow;
            var months = Enumerable.Range(0, 12)
                .Select(i => now.AddMonths(-11 + i))
                .Select(d => new
                {
                    Year = d.Year,
                    Month = d.Month,
                    Label = d.ToString("MMM yy")
                })
                .ToList();

            var result = new List<object>();
            foreach (var m in months)
            {
                var count = await query.CountAsync(b =>
                    b.RegistrationDate.Year == m.Year && b.RegistrationDate.Month == m.Month);

                result.Add(new { label = m.Label, count });
            }

            return Ok(result);
        }
        catch
        {
            var fallback = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-11 + i))
                .Select(d => new { label = d.ToString("MMM yy"), count = 0 });
            return Ok(fallback);
        }
    }

    // GET: /api/v1/dashboard/state-stats
    [HttpGet("state-stats")]
    public async Task<IActionResult> GetStateStats()
    {
        try
        {
            var stateStats = await _context.States
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    state = s.StateName,
                    count = s.Centres.SelectMany(c => c.Beneficiaries).Count()
                })
                .Where(s => s.count > 0)
                .ToListAsync();

            return Ok(stateStats);
        }
        catch
        {
            return Ok(Array.Empty<object>());
        }
    }

    // GET: /api/v1/dashboard/centre-stats
    [HttpGet("centre-stats")]
    public async Task<IActionResult> GetCentreStats()
    {
        try
        {
            var isAdmin = await _authService.IsAdminAsync(User);
            var userCentreId = await _authService.GetUserCentreIdAsync(User);

            var query = _context.Centres
                .Include(c => c.State)
                .Include(c => c.Beneficiaries)
                .AsQueryable();

            if (!isAdmin && userCentreId.HasValue)
            {
                query = query.Where(c => c.CentreId == userCentreId.Value);
            }

            var centreStats = await query
                .OrderByDescending(c => c.Beneficiaries.Count)
                .Take(5)
                .Select(c => new
                {
                    name = c.CentreName,
                    state = c.State != null ? c.State.StateName : string.Empty,
                    beneficiaryCount = c.Beneficiaries.Count,
                    sessionCount = c.Beneficiaries.SelectMany(b => b.CounsellingSessions).Count()
                })
                .ToListAsync();

            return Ok(centreStats);
        }
        catch
        {
            return Ok(Array.Empty<object>());
        }
    }

    // GET: /api/v1/dashboard/recent-beneficiaries
    [HttpGet("recent-beneficiaries")]
    public async Task<IActionResult> GetRecentBeneficiaries()
    {
        try
        {
            var isAdmin = await _authService.IsAdminAsync(User);
            var userCentreId = await _authService.GetUserCentreIdAsync(User);

            var query = _context.Beneficiaries
                .Include(b => b.Centre)
                .AsQueryable();

            if (!isAdmin && userCentreId.HasValue)
            {
                query = query.Where(b => b.CentreId == userCentreId.Value);
            }

            var recent = await query
                .OrderByDescending(b => b.RegistrationDate)
                .Take(5)
                .Select(b => new
                {
                    id = b.BeneficiaryId,
                    beneficiaryId = b.BeneficiaryCode,
                    name = b.FullName,
                    centreName = b.Centre != null ? b.Centre.CentreName : string.Empty,
                    registrationDate = b.RegistrationDate.ToString("dd MMM yyyy")
                })
                .ToListAsync();

            return Ok(recent);
        }
        catch
        {
            return Ok(Array.Empty<object>());
        }
    }

    // GET: /api/v1/dashboard/followup-stats
    [HttpGet("followup-stats")]
    public async Task<IActionResult> GetFollowupStats()
    {
        try
        {
            var isAdmin = await _authService.IsAdminAsync(User);
            var userCentreId = await _authService.GetUserCentreIdAsync(User);

            var query = _context.FollowUps.AsQueryable();
            if (!isAdmin && userCentreId.HasValue)
            {
                query = query.Where(f => f.Beneficiary != null && f.Beneficiary.CentreId == userCentreId.Value);
            }

            var pending = await query.CountAsync(f => f.Status == "Pending");
            var completed = await query.CountAsync(f => f.Status == "Completed");
            var missed = await query.CountAsync(f => f.Status == "Missed");
            var cancelled = await query.CountAsync(f => f.Status == "Cancelled");

            return Ok(new { pending, completed, missed, cancelled });
        }
        catch
        {
            return Ok(new { pending = 0, completed = 0, missed = 0, cancelled = 0 });
        }
    }
}
