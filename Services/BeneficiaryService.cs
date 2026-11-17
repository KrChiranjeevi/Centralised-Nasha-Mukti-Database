using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

/// <summary>
/// Service managing Beneficiary registrations, unique code generation,
/// centre-scoped filtering, and lifecycle updates.
/// </summary>
public class BeneficiaryService : IBeneficiaryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BeneficiaryService> _logger;

    public BeneficiaryService(ApplicationDbContext context, ILogger<BeneficiaryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BeneficiaryIndexViewModel> GetBeneficiariesAsync(
        string? search, int? centreId, string? status, DateTime? startDate, DateTime? endDate, 
        int page, int pageSize, int? userCentreId, bool isAdmin)
    {
        IQueryable<Beneficiary> query = _context.Beneficiaries
            .Include(b => b.Centre)
            .ThenInclude(c => c!.State);

        // 1. Role-based scoping: CentreStaff can only see beneficiaries at their assigned centre
        if (!isAdmin)
        {
            if (userCentreId.HasValue)
            {
                query = query.Where(b => b.CentreId == userCentreId.Value);
            }
            else
            {
                // Unassigned staff sees nothing
                query = query.Where(b => false);
            }
        }
        else if (centreId.HasValue && centreId.Value > 0)
        {
            // Admin filtering by specific centre
            query = query.Where(b => b.CentreId == centreId.Value);
        }

        // 2. Search filter (Beneficiary code or full name)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b => b.BeneficiaryCode.Contains(term) || b.FullName.Contains(term));
        }

        // 3. Status filter
        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(b => b.Status == status);
        }

        // 4. Registration Date range filter
        if (startDate.HasValue)
        {
            query = query.Where(b => b.RegistrationDate >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            query = query.Where(b => b.RegistrationDate <= endDate.Value.Date);
        }

        // Order by newest registration date, then by ID
        query = query.OrderByDescending(b => b.RegistrationDate).ThenByDescending(b => b.BeneficiaryId);

        var projectedQuery = query.Select(b => new BeneficiaryListViewModel
        {
            BeneficiaryId = b.BeneficiaryId,
            BeneficiaryCode = b.BeneficiaryCode,
            FullName = b.FullName,
            CentreId = b.CentreId,
            CentreName = b.Centre != null ? b.Centre.CentreName : "N/A",
            District = b.Centre != null ? b.Centre.District : "N/A",
            StateName = (b.Centre != null && b.Centre.State != null) ? b.Centre.State.StateName : "N/A",
            RegistrationDate = b.RegistrationDate,
            Gender = b.Gender,
            Status = b.Status,
            CreatedAt = b.CreatedAt
        });

        var paginated = await PaginatedList<BeneficiaryListViewModel>.CreateAsync(projectedQuery, page, pageSize);

        // Compute counts based on authorized scope
        IQueryable<Beneficiary> countScope = _context.Beneficiaries;
        if (!isAdmin && userCentreId.HasValue)
        {
            countScope = countScope.Where(b => b.CentreId == userCentreId.Value);
        }
        else if (isAdmin && centreId.HasValue && centreId.Value > 0)
        {
            countScope = countScope.Where(b => b.CentreId == centreId.Value);
        }

        var totalCount = await countScope.CountAsync();
        var activeCount = await countScope.CountAsync(b => b.Status == "Active");
        var centresList = await GetActiveCentresSelectListAsync();

        string? userCentreName = null;
        if (!isAdmin && userCentreId.HasValue)
        {
            userCentreName = await _context.Centres
                .Where(c => c.CentreId == userCentreId.Value)
                .Select(c => c.CentreName)
                .FirstOrDefaultAsync();
        }

        return new BeneficiaryIndexViewModel
        {
            Beneficiaries = paginated,
            Search = search,
            CentreId = centreId,
            Status = status ?? "all",
            StartDate = startDate,
            EndDate = endDate,
            Centres = centresList,
            IsAdmin = isAdmin,
            UserCentreId = userCentreId,
            UserCentreName = userCentreName,
            TotalBeneficiariesCount = totalCount,
            ActiveBeneficiariesCount = activeCount
        };
    }

    public async Task<BeneficiaryDetailsViewModel?> GetBeneficiaryDetailsAsync(int id, int? userCentreId, bool isAdmin)
    {
        var beneficiary = await _context.Beneficiaries
            .Include(b => b.Centre)
            .ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(b => b.BeneficiaryId == id);

        if (beneficiary == null)
            return null;

        // Enforce Centre-Level authorization
        if (!isAdmin && (!userCentreId.HasValue || userCentreId.Value != beneficiary.CentreId))
        {
            _logger.LogWarning("Unauthorized access attempt: User with CentreId {UserCentreId} tried to view Beneficiary {BeneficiaryId} belonging to Centre {TargetCentreId}",
                userCentreId, id, beneficiary.CentreId);
            return null;
        }

        return new BeneficiaryDetailsViewModel
        {
            BeneficiaryId = beneficiary.BeneficiaryId,
            BeneficiaryCode = beneficiary.BeneficiaryCode,
            FullName = beneficiary.FullName,
            CentreId = beneficiary.CentreId,
            CentreName = beneficiary.Centre?.CentreName ?? "N/A",
            District = beneficiary.Centre?.District ?? "N/A",
            StateName = beneficiary.Centre?.State?.StateName ?? "N/A",
            RegistrationDate = beneficiary.RegistrationDate,
            DateOfBirth = beneficiary.DateOfBirth,
            Gender = beneficiary.Gender,
            ContactNumber = beneficiary.ContactNumber,
            Status = beneficiary.Status,
            CreatedAt = beneficiary.CreatedAt,
            UpdatedAt = beneficiary.UpdatedAt,
            CanEdit = isAdmin || (userCentreId.HasValue && userCentreId.Value == beneficiary.CentreId)
        };
    }

    public async Task<Beneficiary?> GetBeneficiaryEntityAsync(int id)
    {
        return await _context.Beneficiaries.FindAsync(id);
    }

    public async Task<(bool Success, string? BeneficiaryCode, string? Error)> CreateBeneficiaryAsync(
        CreateBeneficiaryViewModel model, int? userCentreId, bool isAdmin)
    {
        // 1. Enforce Centre Assignment
        int targetCentreId;
        if (!isAdmin)
        {
            if (!userCentreId.HasValue)
            {
                return (false, null, "Staff account is not assigned to any active centre.");
            }
            targetCentreId = userCentreId.Value; // Never trust browser input for CentreStaff!
        }
        else
        {
            targetCentreId = model.CentreId;
        }

        // 2. Validate that target centre exists and is active
        var centre = await _context.Centres
            .Include(c => c.State)
            .FirstOrDefaultAsync(c => c.CentreId == targetCentreId && c.IsActive);

        if (centre == null)
        {
            return (false, null, "Selected centre does not exist or is currently inactive.");
        }

        // 3. Validate Date Ranges
        if (model.RegistrationDate > DateTime.Today)
        {
            return (false, null, "Registration date cannot be in the future.");
        }
        if (model.DateOfBirth.HasValue && model.DateOfBirth.Value > DateTime.Today)
        {
            return (false, null, "Date of birth cannot be in the future.");
        }

        // 4. Generate Unique Beneficiary Code
        var uniqueCode = await GenerateUniqueBeneficiaryCodeAsync(targetCentreId);

        var beneficiary = new Beneficiary
        {
            BeneficiaryCode = uniqueCode,
            FullName = model.FullName.Trim(),
            CentreId = targetCentreId,
            RegistrationDate = model.RegistrationDate.Date,
            DateOfBirth = model.DateOfBirth?.Date,
            Gender = model.Gender,
            ContactNumber = model.ContactNumber?.Trim(),
            Status = string.IsNullOrWhiteSpace(model.Status) ? "Active" : model.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Beneficiaries.Add(beneficiary);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Beneficiary registered successfully: Code {Code}, Name {Name}, Centre {CentreId}",
            uniqueCode, beneficiary.FullName, targetCentreId);

        return (true, uniqueCode, null);
    }

    public async Task<(bool Success, string? Error)> UpdateBeneficiaryAsync(
        EditBeneficiaryViewModel model, int? userCentreId, bool isAdmin)
    {
        var beneficiary = await _context.Beneficiaries.FindAsync(model.BeneficiaryId);
        if (beneficiary == null)
        {
            return (false, "Beneficiary record not found.");
        }

        // 1. Enforce Centre Authorization
        if (!isAdmin && (!userCentreId.HasValue || userCentreId.Value != beneficiary.CentreId))
        {
            _logger.LogWarning("Unauthorized update attempt on Beneficiary {Id} by CentreStaff {UserCentreId}",
                model.BeneficiaryId, userCentreId);
            return (false, "You do not have permission to modify beneficiaries from another centre.");
        }

        // 2. Centre Assignment Check
        if (isAdmin && model.CentreId != beneficiary.CentreId)
        {
            var centreExists = await _context.Centres.AnyAsync(c => c.CentreId == model.CentreId && c.IsActive);
            if (!centreExists)
            {
                return (false, "Selected target centre is invalid or inactive.");
            }
            beneficiary.CentreId = model.CentreId;
        }

        // 3. Date Validations
        if (model.RegistrationDate > DateTime.Today)
        {
            return (false, "Registration date cannot be in the future.");
        }
        if (model.DateOfBirth.HasValue && model.DateOfBirth.Value > DateTime.Today)
        {
            return (false, "Date of birth cannot be in the future.");
        }

        // 4. Update Permitted Fields (BeneficiaryCode is preserved and cannot be overwritten)
        beneficiary.FullName = model.FullName.Trim();
        beneficiary.RegistrationDate = model.RegistrationDate.Date;
        beneficiary.DateOfBirth = model.DateOfBirth?.Date;
        beneficiary.Gender = model.Gender;
        beneficiary.ContactNumber = model.ContactNumber?.Trim();
        beneficiary.Status = model.Status.Trim();
        beneficiary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Beneficiary {Code} updated successfully.", beneficiary.BeneficiaryCode);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ToggleBeneficiaryStatusAsync(int id, int? userCentreId, bool isAdmin)
    {
        var beneficiary = await _context.Beneficiaries.FindAsync(id);
        if (beneficiary == null)
        {
            return (false, "Beneficiary not found.");
        }

        if (!isAdmin && (!userCentreId.HasValue || userCentreId.Value != beneficiary.CentreId))
        {
            return (false, "Unauthorized: You can only deactivate beneficiaries at your assigned centre.");
        }

        beneficiary.Status = (beneficiary.Status == "Active") ? "Inactive" : "Active";
        beneficiary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Beneficiary {Code} status changed to {Status}", beneficiary.BeneficiaryCode, beneficiary.Status);

        return (true, null);
    }

    public async Task<string> GenerateUniqueBeneficiaryCodeAsync(int centreId)
    {
        var centre = await _context.Centres
            .Include(c => c.State)
            .FirstOrDefaultAsync(c => c.CentreId == centreId);

        var stateName = centre?.State?.StateName ?? "XX";
        var stateCode = GetStateAbbreviation(stateName);
        var year = DateTime.UtcNow.Year;
        var prefix = $"BEN-{stateCode}-{year}-";

        // Count existing records with this prefix
        var count = await _context.Beneficiaries
            .CountAsync(b => b.BeneficiaryCode.StartsWith(prefix));

        int sequence = count + 1;
        string candidateCode;

        // Guaranteed collision prevention loop
        do
        {
            candidateCode = $"{prefix}{sequence:D4}";
            sequence++;
        }
        while (await _context.Beneficiaries.AnyAsync(b => b.BeneficiaryCode == candidateCode));

        return candidateCode;
    }

    public async Task<List<SelectListItem>> GetActiveCentresSelectListAsync()
    {
        return await _context.Centres
            .Where(c => c.IsActive)
            .OrderBy(c => c.CentreName)
            .Select(c => new SelectListItem
            {
                Value = c.CentreId.ToString(),
                Text = $"{c.CentreName} ({c.District})"
            })
            .ToListAsync();
    }

    private static string GetStateAbbreviation(string stateName)
    {
        return stateName.Trim().ToLowerInvariant() switch
        {
            "punjab" => "PB",
            "bihar" => "BR",
            "maharashtra" => "MH",
            "rajasthan" => "RJ",
            "delhi" => "DL",
            "madhya pradesh" => "MP",
            "uttar pradesh" => "UP",
            "haryana" => "HR",
            "west bengal" => "WB",
            "gujarat" => "GJ",
            "karnataka" => "KA",
            "tamil nadu" => "TN",
            _ => stateName.Length >= 2 ? stateName.Substring(0, 2).ToUpperInvariant() : "IN"
        };
    }
}
