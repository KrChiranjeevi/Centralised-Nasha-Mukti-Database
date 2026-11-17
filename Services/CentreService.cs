using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

/// <summary>
/// Implements real database operations and role-based filtering for Centres.
/// </summary>
public class CentreService : ICentreService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CentreService> _logger;

    public CentreService(ApplicationDbContext context, ILogger<CentreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CentreIndexViewModel> GetCentresAsync(
        string? search, int? stateId, string? status, int page, int pageSize, int? userCentreId, bool isAdmin)
    {
        IQueryable<Centre> query = _context.Centres
            .Include(c => c.State)
            .Include(c => c.Beneficiaries);

        // 1. Role-based scoping: CentreStaff can only see their assigned centre
        if (!isAdmin)
        {
            if (userCentreId.HasValue)
            {
                query = query.Where(c => c.CentreId == userCentreId.Value);
            }
            else
            {
                // Unassigned staff sees nothing
                query = query.Where(c => false);
            }
        }

        // 2. Search filter (Centre name or District)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.CentreName.Contains(term) || c.District.Contains(term));
        }

        // 3. State filter
        if (stateId.HasValue && stateId.Value > 0)
        {
            query = query.Where(c => c.StateId == stateId.Value);
        }

        // 4. Status filter
        if (string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => c.IsActive);
        }
        else if (string.Equals(status, "inactive", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => !c.IsActive);
        }

        // Order by active status first, then name
        query = query.OrderByDescending(c => c.IsActive).ThenBy(c => c.CentreName);

        // Project to view model
        var projectedQuery = query.Select(c => new CentreListViewModel
        {
            CentreId = c.CentreId,
            CentreName = c.CentreName,
            StateName = c.State != null ? c.State.StateName : string.Empty,
            District = c.District,
            ContactNumber = c.ContactNumber,
            Email = c.Email,
            IsActive = c.IsActive,
            BeneficiaryCount = c.Beneficiaries.Count,
            CreatedAt = c.CreatedAt
        });

        var paginated = await PaginatedList<CentreListViewModel>.CreateAsync(projectedQuery, page, pageSize);

        var totalCount = await _context.Centres.CountAsync();
        var activeCount = await _context.Centres.CountAsync(c => c.IsActive);
        var statesList = await GetStatesSelectListAsync();

        return new CentreIndexViewModel
        {
            Centres = paginated,
            Search = search,
            StateId = stateId,
            Status = status ?? "all",
            States = statesList,
            IsAdmin = isAdmin,
            TotalCentresCount = totalCount,
            ActiveCentresCount = activeCount
        };
    }

    public async Task<CentreDetailsViewModel?> GetCentreDetailsAsync(int id, int? userCentreId, bool isAdmin)
    {
        // Enforce Centre-Level authorization
        if (!isAdmin && (!userCentreId.HasValue || userCentreId.Value != id))
        {
            _logger.LogWarning("Unauthorized attempt to access Centre {CentreId} by user with CentreId {UserCentreId}",
                id, userCentreId);
            return null;
        }

        var centre = await _context.Centres
            .Include(c => c.State)
            .Include(c => c.Beneficiaries)
            .Include(c => c.StaffMembers)
            .FirstOrDefaultAsync(c => c.CentreId == id);

        if (centre == null)
            return null;

        return new CentreDetailsViewModel
        {
            CentreId = centre.CentreId,
            CentreName = centre.CentreName,
            StateName = centre.State?.StateName ?? "N/A",
            StateId = centre.StateId,
            District = centre.District,
            Address = centre.Address,
            ContactNumber = centre.ContactNumber,
            Email = centre.Email,
            IsActive = centre.IsActive,
            CreatedAt = centre.CreatedAt,
            TotalBeneficiaries = centre.Beneficiaries.Count,
            ActiveBeneficiaries = centre.Beneficiaries.Count(b => b.Status == "Active"),
            TotalStaffMembers = centre.StaffMembers.Count,
            RecentBeneficiaries = centre.Beneficiaries
                .OrderByDescending(b => b.RegistrationDate)
                .Take(5)
                .Select(b => new CentreBeneficiarySummary
                {
                    BeneficiaryId = b.BeneficiaryId,
                    BeneficiaryCode = b.BeneficiaryCode,
                    FullName = b.FullName,
                    RegistrationDate = b.RegistrationDate,
                    Status = b.Status
                })
                .ToList(),
            StaffMembers = centre.StaffMembers
                .Select(s => new CentreStaffSummary
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email ?? string.Empty,
                    IsActive = s.IsActive
                })
                .ToList()
        };
    }

    public async Task<Centre?> GetCentreEntityAsync(int id)
    {
        return await _context.Centres.FindAsync(id);
    }

    public async Task<bool> CreateCentreAsync(CreateCentreViewModel model)
    {
        // 1. Verify that the foreign key StateId exists
        var stateExists = await _context.States.AnyAsync(s => s.StateId == model.StateId);
        if (!stateExists)
        {
            _logger.LogWarning("Cannot create Centre: StateId {StateId} does not exist.", model.StateId);
            return false;
        }

        var centre = new Centre
        {
            CentreName = model.CentreName.Trim(),
            StateId = model.StateId,
            District = model.District.Trim(),
            Address = model.Address.Trim(),
            ContactNumber = model.ContactNumber.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Centres.Add(centre);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created new Centre: {CentreName} in District {District}", centre.CentreName, centre.District);
        return true;
    }

    public async Task<bool> UpdateCentreAsync(EditCentreViewModel model)
    {
        var centre = await _context.Centres.FindAsync(model.CentreId);
        if (centre == null)
            return false;

        // Verify valid StateId
        var stateExists = await _context.States.AnyAsync(s => s.StateId == model.StateId);
        if (!stateExists)
            return false;

        centre.CentreName = model.CentreName.Trim();
        centre.StateId = model.StateId;
        centre.District = model.District.Trim();
        centre.Address = model.Address.Trim();
        centre.ContactNumber = model.ContactNumber.Trim();
        centre.Email = model.Email.Trim().ToLowerInvariant();
        centre.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated Centre {CentreId}: {CentreName}", centre.CentreId, centre.CentreName);
        return true;
    }

    public async Task<bool> ToggleCentreStatusAsync(int id)
    {
        var centre = await _context.Centres.FindAsync(id);
        if (centre == null)
            return false;

        centre.IsActive = !centre.IsActive;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Centre {CentreId} status changed to {Status}", id, centre.IsActive ? "Active" : "Inactive");
        return true;
    }

    public async Task<List<SelectListItem>> GetStatesSelectListAsync()
    {
        return await _context.States
            .Where(s => s.IsActive)
            .OrderBy(s => s.StateName)
            .Select(s => new SelectListItem
            {
                Value = s.StateId.ToString(),
                Text = s.StateName
            })
            .ToListAsync();
    }

    public async Task<int> GetActiveCentresCountAsync()
    {
        return await _context.Centres.CountAsync(c => c.IsActive);
    }

    public async Task<int> GetTotalCentresCountAsync()
    {
        return await _context.Centres.CountAsync();
    }
}
