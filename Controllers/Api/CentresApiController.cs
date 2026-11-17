using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.DTOs;
using NashaMuktiDB.Models;
using NashaMuktiDB.Services;

namespace NashaMuktiDB.Controllers.Api;

[Route("api/v1/centres")]
[ApiController]
[Authorize]
public class CentresApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<CentresApiController> _logger;

    public CentresApiController(
        ApplicationDbContext context,
        ICentreAuthorizationService authService,
        ILogger<CentresApiController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// List centres with optional filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<CentreResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCentres(
        [FromQuery] string? search,
        [FromQuery] int? stateId,
        [FromQuery] string? district,
        [FromQuery] bool? activeOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Centres
            .Include(c => c.State)
            .Include(c => c.Beneficiaries)
            .AsNoTracking()
            .AsQueryable();

        if (activeOnly.HasValue && activeOnly.Value)
        {
            query = query.Where(c => c.IsActive);
        }

        if (stateId.HasValue)
        {
            query = query.Where(c => c.StateId == stateId.Value);
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            query = query.Where(c => c.District.ToLower() == district.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.CentreName.ToLower().Contains(term) || c.District.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(c => c.CentreName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CentreResponseDto
            {
                CentreId = c.CentreId,
                CentreName = c.CentreName,
                StateId = c.StateId,
                StateName = c.State != null ? c.State.StateName : string.Empty,
                District = c.District,
                Address = c.Address,
                ContactNumber = c.ContactNumber,
                Email = c.Email,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                BeneficiariesCount = c.Beneficiaries.Count
            })
            .ToListAsync();

        return Ok(new PaginatedApiResponse<CentreResponseDto>
        {
            Items = items,
            PageIndex = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Retrieve single centre by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CentreResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCentreById(int id)
    {
        var centre = await _context.Centres
            .Include(c => c.State)
            .Include(c => c.Beneficiaries)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CentreId == id);

        if (centre == null)
        {
            return NotFound(ApiResponse<string>.Fail($"Centre with ID {id} not found."));
        }

        var dto = new CentreResponseDto
        {
            CentreId = centre.CentreId,
            CentreName = centre.CentreName,
            StateId = centre.StateId,
            StateName = centre.State != null ? centre.State.StateName : string.Empty,
            District = centre.District,
            Address = centre.Address,
            ContactNumber = centre.ContactNumber,
            Email = centre.Email,
            IsActive = centre.IsActive,
            CreatedAt = centre.CreatedAt,
            BeneficiariesCount = centre.Beneficiaries.Count
        };

        return Ok(ApiResponse<CentreResponseDto>.Ok(dto));
    }

    /// <summary>
    /// Create a new de-addiction centre (Administrator only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CentreResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCentre([FromBody] CreateCentreDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var stateExists = await _context.States.AnyAsync(s => s.StateId == dto.StateId && s.IsActive);
        if (!stateExists)
        {
            return BadRequest(ApiResponse<string>.Fail("Specified State ID is invalid or inactive."));
        }

        var centre = new Centre
        {
            CentreName = dto.CentreName.Trim(),
            StateId = dto.StateId,
            District = dto.District.Trim(),
            Address = dto.Address.Trim(),
            ContactNumber = dto.ContactNumber.Trim(),
            Email = dto.Email.Trim().ToLower(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Centres.Add(centre);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Centre {CentreName} (ID {CentreId}) created via API by {User}", centre.CentreName, centre.CentreId, User.Identity?.Name);

        var stateName = await _context.States.Where(s => s.StateId == centre.StateId).Select(s => s.StateName).FirstOrDefaultAsync() ?? "";

        var responseDto = new CentreResponseDto
        {
            CentreId = centre.CentreId,
            CentreName = centre.CentreName,
            StateId = centre.StateId,
            StateName = stateName,
            District = centre.District,
            Address = centre.Address,
            ContactNumber = centre.ContactNumber,
            Email = centre.Email,
            IsActive = centre.IsActive,
            CreatedAt = centre.CreatedAt,
            BeneficiariesCount = 0
        };

        return CreatedAtAction(nameof(GetCentreById), new { id = centre.CentreId }, ApiResponse<CentreResponseDto>.Ok(responseDto, "Centre registered successfully"));
    }

    /// <summary>
    /// Update an existing centre's information.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CentreResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCentre(int id, [FromBody] UpdateCentreDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var canAccess = await _authService.CanAccessCentreAsync(User, id);
        if (!canAccess)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("Unauthorized: You may not modify centres outside your assigned facility."));
        }

        var centre = await _context.Centres.Include(c => c.State).FirstOrDefaultAsync(c => c.CentreId == id);
        if (centre == null)
        {
            return NotFound(ApiResponse<string>.Fail($"Centre with ID {id} not found."));
        }

        var stateExists = await _context.States.AnyAsync(s => s.StateId == dto.StateId && s.IsActive);
        if (!stateExists)
        {
            return BadRequest(ApiResponse<string>.Fail("Specified State ID is invalid or inactive."));
        }

        centre.CentreName = dto.CentreName.Trim();
        centre.StateId = dto.StateId;
        centre.District = dto.District.Trim();
        centre.Address = dto.Address.Trim();
        centre.ContactNumber = dto.ContactNumber.Trim();
        centre.Email = dto.Email.Trim().ToLower();
        centre.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Centre {CentreId} updated via API by {User}", id, User.Identity?.Name);

        var stateName = await _context.States.Where(s => s.StateId == centre.StateId).Select(s => s.StateName).FirstOrDefaultAsync() ?? "";

        var responseDto = new CentreResponseDto
        {
            CentreId = centre.CentreId,
            CentreName = centre.CentreName,
            StateId = centre.StateId,
            StateName = stateName,
            District = centre.District,
            Address = centre.Address,
            ContactNumber = centre.ContactNumber,
            Email = centre.Email,
            IsActive = centre.IsActive,
            CreatedAt = centre.CreatedAt,
            BeneficiariesCount = await _context.Beneficiaries.CountAsync(b => b.CentreId == id)
        };

        return Ok(ApiResponse<CentreResponseDto>.Ok(responseDto, "Centre updated successfully"));
    }

    /// <summary>
    /// Controlled soft deactivation of a centre (preserves historical clinical records).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateCentre(int id)
    {
        var centre = await _context.Centres.FindAsync(id);
        if (centre == null)
            return NotFound(ApiResponse<string>.Fail($"Centre with ID {id} not found."));

        centre.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Centre {CentreId} deactivated via API by {User}", id, User.Identity?.Name);
        return Ok(ApiResponse<string>.Ok($"Centre '{centre.CentreName}' has been safely deactivated. All historical beneficiary records are preserved."));
    }
}
