using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;

namespace NashaMuktiDB.Controllers.Api;

[Route("api/v1/followups")]
[ApiController]
[Authorize]
public class FollowUpsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<FollowUpsApiController> _logger;

    public FollowUpsApiController(
        ApplicationDbContext context,
        ICentreAuthorizationService authService,
        ILogger<FollowUpsApiController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// List follow-up appointments with support for pending, overdue, and completed tabs.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<FollowUpResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowUps(
        [FromQuery] string? search,
        [FromQuery] string? filterTab, // all, pending, overdue, completed
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 15;

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        var query = _context.FollowUps
            .Include(f => f.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin && userCentreId.HasValue)
        {
            query = query.Where(f => f.Beneficiary != null && f.Beneficiary.CentreId == userCentreId.Value);
        }

        var tab = (filterTab ?? "all").ToLower();
        var today = DateTime.Today;

        switch (tab)
        {
            case "pending":
                query = query.Where(f => f.Status == "Pending" && f.ScheduledDate >= today);
                break;
            case "overdue":
                query = query.Where(f => f.Status == "Pending" && f.ScheduledDate < today);
                break;
            case "completed":
                query = query.Where(f => f.Status == "Completed");
                break;
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
        {
            query = query.Where(f => f.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(f =>
                f.Beneficiary != null && (f.Beneficiary.BeneficiaryCode.ToLower().Contains(term) || f.Beneficiary.FullName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(f => f.ScheduledDate)
            .ThenBy(f => f.FollowUpId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FollowUpResponseDto
            {
                FollowUpId = f.FollowUpId,
                BeneficiaryId = f.BeneficiaryId,
                BeneficiaryCode = f.Beneficiary != null ? f.Beneficiary.BeneficiaryCode : string.Empty,
                BeneficiaryName = f.Beneficiary != null ? f.Beneficiary.FullName : string.Empty,
                CentreName = f.Beneficiary != null && f.Beneficiary.Centre != null ? f.Beneficiary.Centre.CentreName : "N/A",
                District = f.Beneficiary != null && f.Beneficiary.Centre != null ? f.Beneficiary.Centre.District : "N/A",
                StateName = f.Beneficiary != null && f.Beneficiary.Centre != null && f.Beneficiary.Centre.State != null ? f.Beneficiary.Centre.State.StateName : "N/A",
                ScheduledDate = f.ScheduledDate,
                Status = f.Status,
                CompletedDate = f.CompletedDate,
                Notes = f.Notes,
                IsOverdue = f.Status == "Pending" && f.ScheduledDate < today,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync();

        return Ok(new PaginatedApiResponse<FollowUpResponseDto>
        {
            Items = items,
            PageIndex = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Retrieve single follow-up appointment record by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<FollowUpResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowUpById(int id)
    {
        var f = await _context.FollowUps
            .Include(x => x.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FollowUpId == id);

        if (f == null)
            return NotFound(ApiResponse<string>.Fail($"Follow-up appointment with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, f.BeneficiaryId);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Follow-up belongs to a beneficiary of another facility."));

        var today = DateTime.Today;
        var dto = new FollowUpResponseDto
        {
            FollowUpId = f.FollowUpId,
            BeneficiaryId = f.BeneficiaryId,
            BeneficiaryCode = f.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = f.Beneficiary?.FullName ?? string.Empty,
            CentreName = f.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = f.Beneficiary?.Centre?.District ?? "N/A",
            StateName = f.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            ScheduledDate = f.ScheduledDate,
            Status = f.Status,
            CompletedDate = f.CompletedDate,
            Notes = f.Notes,
            IsOverdue = f.Status == "Pending" && f.ScheduledDate < today,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        };

        return Ok(ApiResponse<FollowUpResponseDto>.Ok(dto));
    }

    /// <summary>
    /// Schedule a new follow-up appointment for an authorized beneficiary.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FollowUpResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateFollowUp([FromBody] CreateFollowUpDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var beneficiary = await _context.Beneficiaries
            .Include(b => b.Centre)
                .ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(b => b.BeneficiaryId == dto.BeneficiaryId);

        if (beneficiary == null)
            return BadRequest(ApiResponse<string>.Fail("Specified Beneficiary ID was not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, dto.BeneficiaryId);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: You may only schedule follow-ups for beneficiaries of your assigned facility."));

        var followUp = new FollowUp
        {
            BeneficiaryId = dto.BeneficiaryId,
            ScheduledDate = dto.ScheduledDate.Date,
            Status = dto.Status.Trim(),
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.FollowUps.Add(followUp);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Scheduled Follow-up {FollowUpId} via API by {User}", followUp.FollowUpId, User.Identity?.Name);

        var today = DateTime.Today;
        var responseDto = new FollowUpResponseDto
        {
            FollowUpId = followUp.FollowUpId,
            BeneficiaryId = followUp.BeneficiaryId,
            BeneficiaryCode = beneficiary.BeneficiaryCode,
            BeneficiaryName = beneficiary.FullName,
            CentreName = beneficiary.Centre?.CentreName ?? "N/A",
            District = beneficiary.Centre?.District ?? "N/A",
            StateName = beneficiary.Centre?.State?.StateName ?? "N/A",
            ScheduledDate = followUp.ScheduledDate,
            Status = followUp.Status,
            CompletedDate = null,
            Notes = followUp.Notes,
            IsOverdue = followUp.Status == "Pending" && followUp.ScheduledDate < today,
            CreatedAt = followUp.CreatedAt
        };

        return CreatedAtAction(nameof(GetFollowUpById), new { id = followUp.FollowUpId }, ApiResponse<FollowUpResponseDto>.Ok(responseDto, "Follow-up appointment scheduled successfully"));
    }

    /// <summary>
    /// Update follow-up status, record completion date and recovery observation notes.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<FollowUpResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFollowUp(int id, [FromBody] UpdateFollowUpDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var followUp = await _context.FollowUps
            .Include(f => f.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(f => f.FollowUpId == id);

        if (followUp == null)
            return NotFound(ApiResponse<string>.Fail($"Follow-up with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, followUp.BeneficiaryId);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Follow-up belongs to a beneficiary of another facility."));

        followUp.ScheduledDate = dto.ScheduledDate.Date;
        followUp.Status = dto.Status.Trim();
        followUp.CompletedDate = dto.CompletedDate?.Date ?? (dto.Status == "Completed" ? DateTime.Today : null);
        followUp.Notes = dto.Notes?.Trim();
        followUp.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated Follow-up {FollowUpId} via API by {User}", id, User.Identity?.Name);

        var today = DateTime.Today;
        var responseDto = new FollowUpResponseDto
        {
            FollowUpId = followUp.FollowUpId,
            BeneficiaryId = followUp.BeneficiaryId,
            BeneficiaryCode = followUp.Beneficiary?.BeneficiaryCode ?? "",
            BeneficiaryName = followUp.Beneficiary?.FullName ?? "",
            CentreName = followUp.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = followUp.Beneficiary?.Centre?.District ?? "N/A",
            StateName = followUp.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            ScheduledDate = followUp.ScheduledDate,
            Status = followUp.Status,
            CompletedDate = followUp.CompletedDate,
            Notes = followUp.Notes,
            IsOverdue = followUp.Status == "Pending" && followUp.ScheduledDate < today,
            CreatedAt = followUp.CreatedAt,
            UpdatedAt = followUp.UpdatedAt
        };

        return Ok(ApiResponse<FollowUpResponseDto>.Ok(responseDto, "Follow-up record updated successfully"));
    }
}
