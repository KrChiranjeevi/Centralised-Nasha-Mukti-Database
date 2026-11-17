using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;

namespace NashaMuktiDB.Controllers.Api;

[Route("api/v1/counselling")]
[ApiController]
[Authorize]
public class CounsellingApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<CounsellingApiController> _logger;

    public CounsellingApiController(
        ApplicationDbContext context,
        ICentreAuthorizationService authService,
        ILogger<CounsellingApiController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// List counselling sessions with privacy protection (clinical notes excluded from list).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<CounsellingListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions(
        [FromQuery] string? search,
        [FromQuery] int? beneficiaryId,
        [FromQuery] string? sessionType,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 15;

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        var query = _context.CounsellingSessions
            .Include(s => s.Beneficiary)
                .ThenInclude(b => b!.Centre)
            .AsNoTracking()
            .AsQueryable();

        // Enforce CentreStaff boundary
        if (!isAdmin && userCentreId.HasValue)
        {
            query = query.Where(s => s.Beneficiary != null && s.Beneficiary.CentreId == userCentreId.Value);
        }

        if (beneficiaryId.HasValue)
        {
            query = query.Where(s => s.BeneficiaryId == beneficiaryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(sessionType) && sessionType.ToLower() != "all")
        {
            query = query.Where(s => s.SessionType == sessionType);
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
        {
            query = query.Where(s => s.SessionStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                (s.Beneficiary != null && (s.Beneficiary.BeneficiaryCode.ToLower().Contains(term) || s.Beneficiary.FullName.ToLower().Contains(term))) ||
                (s.CounsellorName != null && s.CounsellorName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(s => s.SessionDate)
            .ThenByDescending(s => s.SessionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new CounsellingListItemDto
            {
                SessionId = s.SessionId,
                BeneficiaryId = s.BeneficiaryId,
                BeneficiaryCode = s.Beneficiary != null ? s.Beneficiary.BeneficiaryCode : string.Empty,
                BeneficiaryName = s.Beneficiary != null ? s.Beneficiary.FullName : string.Empty,
                CentreName = s.Beneficiary != null && s.Beneficiary.Centre != null ? s.Beneficiary.Centre.CentreName : "N/A",
                SessionDate = s.SessionDate,
                SessionType = s.SessionType,
                CounsellorName = s.CounsellorName,
                SessionStatus = s.SessionStatus,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(new PaginatedApiResponse<CounsellingListItemDto>
        {
            Items = items,
            PageIndex = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Retrieve single counselling session details including confidential clinical notes (strictly access-restricted).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CounsellingDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionById(int id)
    {
        var s = await _context.CounsellingSessions
            .Include(x => x.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionId == id);

        if (s == null)
            return NotFound(ApiResponse<string>.Fail($"Counselling session with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, s.BeneficiaryId);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Session belongs to a beneficiary of another facility."));

        var dto = new CounsellingDetailDto
        {
            SessionId = s.SessionId,
            BeneficiaryId = s.BeneficiaryId,
            BeneficiaryCode = s.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = s.Beneficiary?.FullName ?? string.Empty,
            CentreName = s.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = s.Beneficiary?.Centre?.District ?? "N/A",
            StateName = s.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            SessionDate = s.SessionDate,
            SessionType = s.SessionType,
            CounsellorName = s.CounsellorName,
            SessionStatus = s.SessionStatus,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };

        return Ok(ApiResponse<CounsellingDetailDto>.Ok(dto));
    }

    /// <summary>
    /// Record a new counselling session for an authorized beneficiary.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CounsellingDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateSession([FromBody] CreateCounsellingDto dto)
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
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: You may only record counselling sessions for beneficiaries of your assigned facility."));

        var session = new CounsellingSession
        {
            BeneficiaryId = dto.BeneficiaryId,
            SessionDate = dto.SessionDate.Date,
            SessionType = dto.SessionType.Trim(),
            CounsellorName = dto.CounsellorName?.Trim(),
            SessionStatus = dto.SessionStatus.Trim(),
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.CounsellingSessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded Counselling Session {SessionId} via API by {User}", session.SessionId, User.Identity?.Name);

        var responseDto = new CounsellingDetailDto
        {
            SessionId = session.SessionId,
            BeneficiaryId = session.BeneficiaryId,
            BeneficiaryCode = beneficiary.BeneficiaryCode,
            BeneficiaryName = beneficiary.FullName,
            CentreName = beneficiary.Centre?.CentreName ?? "N/A",
            District = beneficiary.Centre?.District ?? "N/A",
            StateName = beneficiary.Centre?.State?.StateName ?? "N/A",
            SessionDate = session.SessionDate,
            SessionType = session.SessionType,
            CounsellorName = session.CounsellorName,
            SessionStatus = session.SessionStatus,
            Notes = session.Notes,
            CreatedAt = session.CreatedAt
        };

        return CreatedAtAction(nameof(GetSessionById), new { id = session.SessionId }, ApiResponse<CounsellingDetailDto>.Ok(responseDto, "Counselling session recorded successfully"));
    }

    /// <summary>
    /// Update session notes, status, and therapist information.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CounsellingDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSession(int id, [FromBody] UpdateCounsellingDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var session = await _context.CounsellingSessions
            .Include(s => s.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(s => s.SessionId == id);

        if (session == null)
            return NotFound(ApiResponse<string>.Fail($"Counselling session with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, session.BeneficiaryId);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Session belongs to a beneficiary of another facility."));

        session.SessionDate = dto.SessionDate.Date;
        session.SessionType = dto.SessionType.Trim();
        session.CounsellorName = dto.CounsellorName?.Trim();
        session.SessionStatus = dto.SessionStatus.Trim();
        session.Notes = dto.Notes?.Trim();
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated Counselling Session {SessionId} via API by {User}", id, User.Identity?.Name);

        var responseDto = new CounsellingDetailDto
        {
            SessionId = session.SessionId,
            BeneficiaryId = session.BeneficiaryId,
            BeneficiaryCode = session.Beneficiary?.BeneficiaryCode ?? "",
            BeneficiaryName = session.Beneficiary?.FullName ?? "",
            CentreName = session.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = session.Beneficiary?.Centre?.District ?? "N/A",
            StateName = session.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            SessionDate = session.SessionDate,
            SessionType = session.SessionType,
            CounsellorName = session.CounsellorName,
            SessionStatus = session.SessionStatus,
            Notes = session.Notes,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };

        return Ok(ApiResponse<CounsellingDetailDto>.Ok(responseDto, "Counselling session updated successfully"));
    }
}
