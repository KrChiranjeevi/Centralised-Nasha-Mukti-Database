using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers.Api;

[Route("api/v1/beneficiaries")]
[ApiController]
[Authorize]
public class BeneficiariesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<BeneficiariesApiController> _logger;

    public BeneficiariesApiController(
        ApplicationDbContext context,
        IBeneficiaryService beneficiaryService,
        ICentreAuthorizationService authService,
        ILogger<BeneficiariesApiController> logger)
    {
        _context = context;
        _beneficiaryService = beneficiaryService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// List beneficiaries with search and status filters (scoped to caller's facility for CentreStaff).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<BeneficiaryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBeneficiaries(
        [FromQuery] string? search,
        [FromQuery] int? centreId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 15;

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        var query = _context.Beneficiaries
            .Include(b => b.Centre)
                .ThenInclude(c => c!.State)
            .AsNoTracking()
            .AsQueryable();

        // Enforce CentreStaff boundary
        if (!isAdmin && userCentreId.HasValue)
        {
            query = query.Where(b => b.CentreId == userCentreId.Value);
        }
        else if (centreId.HasValue)
        {
            query = query.Where(b => b.CentreId == centreId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
        {
            query = query.Where(b => b.Status.ToLower() == status.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b => b.BeneficiaryCode.ToLower().Contains(term) || b.FullName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(b => b.RegistrationDate)
            .ThenByDescending(b => b.BeneficiaryId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BeneficiaryResponseDto
            {
                BeneficiaryId = b.BeneficiaryId,
                BeneficiaryCode = b.BeneficiaryCode,
                FullName = b.FullName,
                CentreId = b.CentreId,
                CentreName = b.Centre != null ? b.Centre.CentreName : string.Empty,
                District = b.Centre != null ? b.Centre.District : string.Empty,
                StateName = b.Centre != null && b.Centre.State != null ? b.Centre.State.StateName : string.Empty,
                RegistrationDate = b.RegistrationDate,
                DateOfBirth = b.DateOfBirth,
                Gender = b.Gender,
                ContactNumber = b.ContactNumber,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync();

        return Ok(new PaginatedApiResponse<BeneficiaryResponseDto>
        {
            Items = items,
            PageIndex = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Retrieve single beneficiary by ID (verifies facility authorization).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BeneficiaryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBeneficiaryById(int id)
    {
        var b = await _context.Beneficiaries
            .Include(x => x.Centre)
                .ThenInclude(c => c!.State)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BeneficiaryId == id);

        if (b == null)
            return NotFound(ApiResponse<string>.Fail($"Beneficiary with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, id);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Beneficiary belongs to another centre."));

        var dto = new BeneficiaryResponseDto
        {
            BeneficiaryId = b.BeneficiaryId,
            BeneficiaryCode = b.BeneficiaryCode,
            FullName = b.FullName,
            CentreId = b.CentreId,
            CentreName = b.Centre?.CentreName ?? string.Empty,
            District = b.Centre?.District ?? string.Empty,
            StateName = b.Centre?.State?.StateName ?? string.Empty,
            RegistrationDate = b.RegistrationDate,
            DateOfBirth = b.DateOfBirth,
            Gender = b.Gender,
            ContactNumber = b.ContactNumber,
            Status = b.Status,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };

        return Ok(ApiResponse<BeneficiaryResponseDto>.Ok(dto));
    }

    /// <summary>
    /// Register a new beneficiary. Auto-generates a standardized BeneficiaryCode.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BeneficiaryResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterBeneficiary([FromBody] RegisterBeneficiaryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        int targetCentreId;
        if (!isAdmin && userCentreId.HasValue)
        {
            targetCentreId = userCentreId.Value;
        }
        else
        {
            if (!dto.CentreId.HasValue || dto.CentreId.Value <= 0)
            {
                return BadRequest(ApiResponse<string>.Fail("CentreId is required for Administrator callers."));
            }
            targetCentreId = dto.CentreId.Value;
        }

        var centre = await _context.Centres.Include(c => c.State).FirstOrDefaultAsync(c => c.CentreId == targetCentreId && c.IsActive);
        if (centre == null)
            return BadRequest(ApiResponse<string>.Fail("Target centre was not found or is currently inactive."));

        var model = new CreateBeneficiaryViewModel
        {
            FullName = dto.FullName.Trim(),
            CentreId = targetCentreId,
            RegistrationDate = dto.RegistrationDate ?? DateTime.Today,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender?.Trim(),
            ContactNumber = dto.ContactNumber?.Trim()
        };

        var (success, code, error) = await _beneficiaryService.CreateBeneficiaryAsync(model, userCentreId, isAdmin);
        if (!success)
            return BadRequest(ApiResponse<string>.Fail(error ?? "Failed to register beneficiary"));

        var created = await _context.Beneficiaries
            .Include(b => b.Centre)
                .ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(b => b.BeneficiaryCode == code);

        var responseDto = new BeneficiaryResponseDto
        {
            BeneficiaryId = created!.BeneficiaryId,
            BeneficiaryCode = created.BeneficiaryCode,
            FullName = created.FullName,
            CentreId = created.CentreId,
            CentreName = created.Centre?.CentreName ?? string.Empty,
            District = created.Centre?.District ?? string.Empty,
            StateName = created.Centre?.State?.StateName ?? string.Empty,
            RegistrationDate = created.RegistrationDate,
            DateOfBirth = created.DateOfBirth,
            Gender = created.Gender,
            ContactNumber = created.ContactNumber,
            Status = created.Status,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };

        return CreatedAtAction(nameof(GetBeneficiaryById), new { id = created.BeneficiaryId }, ApiResponse<BeneficiaryResponseDto>.Ok(responseDto, "Beneficiary registered successfully"));
    }

    /// <summary>
    /// Update beneficiary profile details.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BeneficiaryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBeneficiary(int id, [FromBody] UpdateBeneficiaryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var beneficiary = await _context.Beneficiaries.Include(b => b.Centre).FirstOrDefaultAsync(b => b.BeneficiaryId == id);
        if (beneficiary == null)
            return NotFound(ApiResponse<string>.Fail($"Beneficiary with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, id);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Beneficiary belongs to another facility."));

        beneficiary.FullName = dto.FullName.Trim();
        beneficiary.RegistrationDate = dto.RegistrationDate.Date;
        beneficiary.DateOfBirth = dto.DateOfBirth?.Date;
        beneficiary.Gender = dto.Gender?.Trim();
        beneficiary.ContactNumber = dto.ContactNumber?.Trim();
        beneficiary.Status = dto.Status.Trim();
        beneficiary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Beneficiary {BeneficiaryId} updated via API by {User}", id, User.Identity?.Name);

        var updated = await _context.Beneficiaries
            .Include(b => b.Centre)
                .ThenInclude(c => c!.State)
            .FirstAsync(b => b.BeneficiaryId == id);

        var responseDto = new BeneficiaryResponseDto
        {
            BeneficiaryId = updated.BeneficiaryId,
            BeneficiaryCode = updated.BeneficiaryCode,
            FullName = updated.FullName,
            CentreId = updated.CentreId,
            CentreName = updated.Centre?.CentreName ?? string.Empty,
            District = updated.Centre?.District ?? string.Empty,
            StateName = updated.Centre?.State?.StateName ?? string.Empty,
            RegistrationDate = updated.RegistrationDate,
            DateOfBirth = updated.DateOfBirth,
            Gender = updated.Gender,
            ContactNumber = updated.ContactNumber,
            Status = updated.Status,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };

        return Ok(ApiResponse<BeneficiaryResponseDto>.Ok(responseDto, "Beneficiary profile updated successfully"));
    }

    /// <summary>
    /// Controlled deactivation / discharge of a beneficiary (preserves historical clinical records).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateBeneficiary(int id)
    {
        var b = await _context.Beneficiaries.FindAsync(id);
        if (b == null)
            return NotFound(ApiResponse<string>.Fail($"Beneficiary with ID {id} not found."));

        var canAccess = await _authService.CanAccessBeneficiaryAsync(User, id);
        if (!canAccess)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Access Denied: Beneficiary belongs to another facility."));

        b.Status = "Inactive";
        b.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Beneficiary {BeneficiaryCode} deactivated via API by {User}", b.BeneficiaryCode, User.Identity?.Name);
        return Ok(ApiResponse<string>.Ok($"Beneficiary '{b.BeneficiaryCode}' has been deactivated. All historical counselling, intervention, and follow-up records are preserved."));
    }
}
