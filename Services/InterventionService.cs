using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

public class InterventionService : IInterventionService
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<InterventionService> _logger;

    private static readonly string[] PredefinedTypes =
    {
        "Medical Detoxification",
        "Pharmacotherapy / Substitution",
        "Psychosocial Rehabilitation",
        "Skill Development & Vocational",
        "Nutritional & Physical Wellness",
        "Specialist Medical Referral"
    };

    private static readonly string[] PredefinedStatuses =
    {
        "In Progress",
        "Completed",
        "Discontinued",
        "Referred"
    };

    public InterventionService(
        ApplicationDbContext context,
        ICentreAuthorizationService authService,
        ILogger<InterventionService> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    public async Task<InterventionIndexViewModel> GetInterventionsAsync(
        ClaimsPrincipal user,
        string? search,
        int? beneficiaryId,
        string? interventionType,
        string? status,
        int page = 1,
        int pageSize = 10)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var query = _context.Interventions
            .Include(i => i.Beneficiary)
                .ThenInclude(b => b!.Centre)
            .AsNoTracking()
            .AsQueryable();

        // Enforce CentreStaff boundary
        if (!isAdmin && userCentreId.HasValue)
        {
            query = query.Where(i => i.Beneficiary != null && i.Beneficiary.CentreId == userCentreId.Value);
        }

        // Beneficiary Filter
        string? beneficiaryName = null;
        if (beneficiaryId.HasValue)
        {
            query = query.Where(i => i.BeneficiaryId == beneficiaryId.Value);
            beneficiaryName = await _context.Beneficiaries
                .Where(b => b.BeneficiaryId == beneficiaryId.Value)
                .Select(b => $"{b.FullName} ({b.BeneficiaryCode})")
                .FirstOrDefaultAsync();
        }

        // Search Filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                (i.Beneficiary != null && (i.Beneficiary.BeneficiaryCode.ToLower().Contains(term) || i.Beneficiary.FullName.ToLower().Contains(term))) ||
                (i.ResponsibleStaffName != null && i.ResponsibleStaffName.ToLower().Contains(term)));
        }

        // Category Filter
        if (!string.IsNullOrWhiteSpace(interventionType) && interventionType != "all")
        {
            query = query.Where(i => i.InterventionType == interventionType);
        }

        // Status Filter
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            query = query.Where(i => i.Status == status);
        }

        // Counts
        var baseScoped = _context.Interventions.AsNoTracking().AsQueryable();
        if (!isAdmin && userCentreId.HasValue)
        {
            baseScoped = baseScoped.Where(i => i.Beneficiary != null && i.Beneficiary.CentreId == userCentreId.Value);
        }

        var totalCount = await baseScoped.CountAsync();
        var completedCount = await baseScoped.CountAsync(i => i.Status == "Completed");
        var inProgressCount = await baseScoped.CountAsync(i => i.Status == "In Progress");

        query = query.OrderByDescending(i => i.ServiceDate).ThenByDescending(i => i.InterventionId);

        var projected = query.Select(i => new InterventionListItemViewModel
        {
            InterventionId = i.InterventionId,
            BeneficiaryId = i.BeneficiaryId,
            BeneficiaryCode = i.Beneficiary != null ? i.Beneficiary.BeneficiaryCode : string.Empty,
            BeneficiaryName = i.Beneficiary != null ? i.Beneficiary.FullName : string.Empty,
            CentreName = i.Beneficiary != null && i.Beneficiary.Centre != null ? i.Beneficiary.Centre.CentreName : "N/A",
            InterventionType = i.InterventionType,
            ServiceDate = i.ServiceDate,
            Status = i.Status,
            ResponsibleStaffName = i.ResponsibleStaffName,
            CreatedAt = i.CreatedAt
        });

        var paginated = await PaginatedList<InterventionListItemViewModel>.CreateAsync(projected, page, pageSize);

        var typesList = PredefinedTypes.Select(t => new SelectListItem
        {
            Value = t,
            Text = t,
            Selected = t == interventionType
        }).ToList();

        var statusesList = PredefinedStatuses.Select(st => new SelectListItem
        {
            Value = st,
            Text = st,
            Selected = st == status
        }).ToList();

        return new InterventionIndexViewModel
        {
            Interventions = paginated,
            SearchQuery = search,
            BeneficiaryId = beneficiaryId,
            BeneficiaryName = beneficiaryName,
            InterventionType = interventionType ?? "all",
            Status = status ?? "all",
            TotalInterventionsCount = totalCount,
            CompletedCount = completedCount,
            InProgressCount = inProgressCount,
            TypesList = typesList,
            StatusesList = statusesList
        };
    }

    public async Task<InterventionDetailsViewModel?> GetInterventionDetailsAsync(int interventionId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var intervention = await _context.Interventions
            .Include(i => i.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InterventionId == interventionId);

        if (intervention == null)
            return null;

        if (!isAdmin && userCentreId.HasValue && intervention.Beneficiary?.CentreId != userCentreId.Value)
        {
            _logger.LogWarning("Unauthorized access: User centre {UserCentreId} attempted to view Intervention {InterventionId} from centre {CentreId}",
                userCentreId, interventionId, intervention.Beneficiary?.CentreId);
            return null;
        }

        return new InterventionDetailsViewModel
        {
            InterventionId = intervention.InterventionId,
            BeneficiaryId = intervention.BeneficiaryId,
            BeneficiaryCode = intervention.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = intervention.Beneficiary?.FullName ?? string.Empty,
            CentreName = intervention.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = intervention.Beneficiary?.Centre?.District ?? "N/A",
            StateName = intervention.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            InterventionType = intervention.InterventionType,
            ServiceDate = intervention.ServiceDate,
            Status = intervention.Status,
            ResponsibleStaffName = intervention.ResponsibleStaffName,
            Notes = intervention.Notes,
            CreatedAt = intervention.CreatedAt,
            UpdatedAt = intervention.UpdatedAt,
            CanEdit = isAdmin || (userCentreId.HasValue && intervention.Beneficiary?.CentreId == userCentreId.Value)
        };
    }

    public async Task<CreateInterventionViewModel> GetCreateViewModelAsync(ClaimsPrincipal user, int? beneficiaryId = null)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var model = new CreateInterventionViewModel
        {
            ServiceDate = DateTime.Today,
            InterventionType = "Medical Detoxification",
            Status = "In Progress",
            TypesList = PredefinedTypes.Select(t => new SelectListItem { Value = t, Text = t }).ToList(),
            StatusesList = PredefinedStatuses.Select(st => new SelectListItem { Value = st, Text = st }).ToList()
        };

        var benQuery = _context.Beneficiaries.AsNoTracking().Where(b => b.Status == "Active");
        if (!isAdmin && userCentreId.HasValue)
        {
            benQuery = benQuery.Where(b => b.CentreId == userCentreId.Value);
        }

        if (beneficiaryId.HasValue)
        {
            var ben = await _context.Beneficiaries
                .Include(b => b.Centre)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BeneficiaryId == beneficiaryId.Value);

            if (ben != null && (isAdmin || ben.CentreId == userCentreId))
            {
                model.BeneficiaryId = ben.BeneficiaryId;
                model.BeneficiaryCode = ben.BeneficiaryCode;
                model.BeneficiaryName = ben.FullName;
                model.CentreName = ben.Centre?.CentreName;
            }
        }

        model.BeneficiariesList = await benQuery
            .OrderBy(b => b.FullName)
            .Select(b => new SelectListItem
            {
                Value = b.BeneficiaryId.ToString(),
                Text = $"{b.FullName} ({b.BeneficiaryCode})"
            })
            .ToListAsync();

        return model;
    }

    public async Task<EditInterventionViewModel?> GetEditViewModelAsync(int interventionId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var item = await _context.Interventions
            .Include(i => i.Beneficiary)
                .ThenInclude(b => b!.Centre)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InterventionId == interventionId);

        if (item == null)
            return null;

        if (!isAdmin && userCentreId.HasValue && item.Beneficiary?.CentreId != userCentreId.Value)
        {
            return null;
        }

        return new EditInterventionViewModel
        {
            InterventionId = item.InterventionId,
            BeneficiaryId = item.BeneficiaryId,
            BeneficiaryCode = item.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = item.Beneficiary?.FullName ?? string.Empty,
            CentreName = item.Beneficiary?.Centre?.CentreName ?? "N/A",
            InterventionType = item.InterventionType,
            ServiceDate = item.ServiceDate,
            Status = item.Status,
            ResponsibleStaffName = item.ResponsibleStaffName,
            Notes = item.Notes,
            TypesList = PredefinedTypes.Select(t => new SelectListItem
            {
                Value = t,
                Text = t,
                Selected = t == item.InterventionType
            }).ToList(),
            StatusesList = PredefinedStatuses.Select(st => new SelectListItem
            {
                Value = st,
                Text = st,
                Selected = st == item.Status
            }).ToList()
        };
    }

    public async Task<(bool Success, int InterventionId, string? Error)> CreateInterventionAsync(
        CreateInterventionViewModel model,
        ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var beneficiary = await _context.Beneficiaries.FindAsync(model.BeneficiaryId);
        if (beneficiary == null)
        {
            return (false, 0, "Selected beneficiary was not found.");
        }

        if (!isAdmin && userCentreId.HasValue && beneficiary.CentreId != userCentreId.Value)
        {
            _logger.LogWarning("Access Denied: User from centre {UserCentreId} tried to record intervention for beneficiary {BeneficiaryId} belonging to centre {TargetCentreId}",
                userCentreId, beneficiary.BeneficiaryId, beneficiary.CentreId);
            return (false, 0, "Unauthorized: You may only record interventions for beneficiaries of your assigned centre.");
        }

        var intervention = new Intervention
        {
            BeneficiaryId = model.BeneficiaryId,
            InterventionType = model.InterventionType.Trim(),
            ServiceDate = model.ServiceDate.Date,
            Status = model.Status.Trim(),
            ResponsibleStaffName = model.ResponsibleStaffName?.Trim(),
            Notes = model.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Interventions.Add(intervention);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded Intervention {InterventionId} ({Type}) for Beneficiary {BeneficiaryCode} by user {User}",
            intervention.InterventionId, intervention.InterventionType, beneficiary.BeneficiaryCode, user.Identity?.Name);

        return (true, intervention.InterventionId, null);
    }

    public async Task<(bool Success, string? Error)> UpdateInterventionAsync(
        EditInterventionViewModel model,
        ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var intervention = await _context.Interventions
            .Include(i => i.Beneficiary)
            .FirstOrDefaultAsync(i => i.InterventionId == model.InterventionId);

        if (intervention == null)
        {
            return (false, "Intervention not found.");
        }

        if (!isAdmin && userCentreId.HasValue && intervention.Beneficiary?.CentreId != userCentreId.Value)
        {
            return (false, "Unauthorized: You cannot edit interventions belonging to another centre.");
        }

        intervention.InterventionType = model.InterventionType.Trim();
        intervention.ServiceDate = model.ServiceDate.Date;
        intervention.Status = model.Status.Trim();
        intervention.ResponsibleStaffName = model.ResponsibleStaffName?.Trim();
        intervention.Notes = model.Notes?.Trim();
        intervention.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated Intervention {InterventionId} by user {User}", intervention.InterventionId, user.Identity?.Name);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelInterventionAsync(int interventionId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var intervention = await _context.Interventions
            .Include(i => i.Beneficiary)
            .FirstOrDefaultAsync(i => i.InterventionId == interventionId);

        if (intervention == null)
            return (false, "Intervention not found.");

        if (!isAdmin && userCentreId.HasValue && intervention.Beneficiary?.CentreId != userCentreId.Value)
            return (false, "Unauthorized action.");

        intervention.Status = "Discontinued";
        intervention.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Discontinued Intervention {InterventionId} by user {User}", interventionId, user.Identity?.Name);
        return (true, null);
    }
}

