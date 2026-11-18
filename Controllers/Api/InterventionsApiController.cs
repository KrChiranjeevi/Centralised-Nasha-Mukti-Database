using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;

namespace NashaMuktiDB.Controllers.Api;

[Route("api/v1/interventions")]
[ApiController]
[Authorize]
public class InterventionsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<InterventionsApiController> _logger;

    public InterventionsApiController(
        ApplicationDbContext context,
        ICentreAuthorizationService authService,
        ILogger<InterventionsApiController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// List clinical treatment interventions with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<InterventionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInterventions(
        [FromQuery] string? search,
        [FromQuery] int? beneficiaryId,
        [FromQuery] string? interventionType,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 15;

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        var query = _context.Interventions
            .Include(i => i.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin && userCentreId.HasValue)
        {
            query = query.Where(i => i.Beneficiary != null && i.Beneficiary.CentreId == userCentreId.Value);
        }

        if (beneficiaryId.HasValue)
        {
            query = query.Where(i => i.BeneficiaryId == beneficiaryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(interventionType) && interventionType.ToLower() != "all")
        {
            query = query.Where(i => i.InterventionType == interventionType);
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
        {
            query = query.Where(i => i.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                (i.Beneficiary != null && (i.Beneficiary.BeneficiaryCode.ToLower().Contains(term) || i.Beneficiary.FullName.ToLower().Contains(term))) ||
                (i.ResponsibleStaffName != null && i.ResponsibleStaffName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(i => i.ServiceDate)
            .ThenByDescending(i => i.InterventionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InterventionResponseDto
            {
                InterventionId = i.InterventionId,
                BeneficiaryId = i.BeneficiaryId,
                BeneficiaryCode = i.Beneficiary != null ? i.Beneficiary.BeneficiaryCode : string.Empty,
                BeneficiaryName = i.Beneficiary != null ? i.Beneficiary.FullName : string.Empty,
                CentreName = i.Beneficiary != null && i.Beneficiary.Centre != null ? i.Beneficiary.Centre.CentreName : "N/A",
                District = i.Beneficiary != null && i.Beneficiary.Centre != null ? i.Beneficiary.Centre.District : "N/A",
                StateName = i.Beneficiary != null && i.Beneficiary.Centre != null && i.Beneficiary.Centre.State != null ? i.Beneficiary.Centre.State.StateName : "N/A",
                InterventionType = i.InterventionType,
                ServiceDate = i.ServiceDate,
                Status = i.Status,
                ResponsibleStaffName = i.ResponsibleStaffName,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();

        return Ok(new PaginatedApiResponse<InterventionResponseDto>
        {
            Items = items,
            PageIndex = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Retrieve single intervention protocol details by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InterventionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInterventionById(int id)
    {
        var i = await _context.Interventions
            .Include(x => x.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InterventionId == id);

        if (i == null)
            return NotFound(ApiResponse<string>.Fail($"Intervention with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, i.BeneficiaryId);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Intervention belongs to a beneficiary of another facility."));

        var dto = new InterventionResponseDto
        {
            InterventionId = i.InterventionId,
            BeneficiaryId = i.BeneficiaryId,
            BeneficiaryCode = i.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = i.Beneficiary?.FullName ?? string.Empty,
            CentreName = i.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = i.Beneficiary?.Centre?.District ?? "N/A",
            StateName = i.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            InterventionType = i.InterventionType,
            ServiceDate = i.ServiceDate,
            Status = i.Status,
            ResponsibleStaffName = i.ResponsibleStaffName,
            Notes = i.Notes,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt
        };

        return Ok(ApiResponse<InterventionResponseDto>.Ok(dto));
    }

    /// <summary>
    /// Record a new medical or psychosocial intervention for an authorized beneficiary.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InterventionResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateIntervention([FromBody] CreateInterventionDto dto)
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
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: You may only record interventions for beneficiaries of your assigned facility."));

        var intervention = new Intervention
        {
            BeneficiaryId = dto.BeneficiaryId,
            InterventionType = dto.InterventionType.Trim(),
            ServiceDate = dto.ServiceDate.Date,
            Status = dto.Status.Trim(),
            ResponsibleStaffName = dto.ResponsibleStaffName?.Trim(),
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Interventions.Add(intervention);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded Intervention {InterventionId} via API by {User}", intervention.InterventionId, User.Identity?.Name);

        var responseDto = new InterventionResponseDto
        {
            InterventionId = intervention.InterventionId,
            BeneficiaryId = intervention.BeneficiaryId,
            BeneficiaryCode = beneficiary.BeneficiaryCode,
            BeneficiaryName = beneficiary.FullName,
            CentreName = beneficiary.Centre?.CentreName ?? "N/A",
            District = beneficiary.Centre?.District ?? "N/A",
            StateName = beneficiary.Centre?.State?.StateName ?? "N/A",
            InterventionType = intervention.InterventionType,
            ServiceDate = intervention.ServiceDate,
            Status = intervention.Status,
            ResponsibleStaffName = intervention.ResponsibleStaffName,
            Notes = intervention.Notes,
            CreatedAt = intervention.CreatedAt
        };

        return CreatedAtAction(nameof(GetInterventionById), new { id = intervention.InterventionId }, ApiResponse<InterventionResponseDto>.Ok(responseDto, "Intervention recorded successfully"));
    }

    /// <summary>
    /// Update intervention details and protocol notes.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InterventionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIntervention(int id, [FromBody] UpdateInterventionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var intervention = await _context.Interventions
            .Include(i => i.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(i => i.InterventionId == id);

        if (intervention == null)
            return NotFound(ApiResponse<string>.Fail($"Intervention with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, intervention.BeneficiaryId);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Intervention belongs to a beneficiary of another facility."));

        intervention.InterventionType = dto.InterventionType.Trim();
        intervention.ServiceDate = dto.ServiceDate.Date;
        intervention.Status = dto.Status.Trim();
        intervention.ResponsibleStaffName = dto.ResponsibleStaffName?.Trim();
        intervention.Notes = dto.Notes?.Trim();
        intervention.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated Intervention {InterventionId} via API by {User}", id, User.Identity?.Name);

        var responseDto = new InterventionResponseDto
        {
            InterventionId = intervention.InterventionId,
            BeneficiaryId = intervention.BeneficiaryId,
            BeneficiaryCode = intervention.Beneficiary?.BeneficiaryCode ?? "",
            BeneficiaryName = intervention.Beneficiary?.FullName ?? "",
            CentreName = intervention.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = intervention.Beneficiary?.Centre?.District ?? "N/A",
            StateName = intervention.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            InterventionType = intervention.InterventionType,
            ServiceDate = intervention.ServiceDate,
            Status = intervention.Status,
            ResponsibleStaffName = intervention.ResponsibleStaffName,
            Notes = intervention.Notes,
            CreatedAt = intervention.CreatedAt,
            UpdatedAt = intervention.UpdatedAt
        };

        return Ok(ApiResponse<InterventionResponseDto>.Ok(responseDto, "Intervention updated successfully"));
    }
}

