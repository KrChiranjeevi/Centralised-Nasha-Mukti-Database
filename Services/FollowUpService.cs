using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

public class FollowUpService : IFollowUpService
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<FollowUpService> _logger;

    private static readonly string[] PredefinedStatuses =
    {
        "Pending",
        "Completed",
        "Missed",
        "Cancelled"
    };

    public FollowUpService(
        ApplicationDbContext context,
        ICentreAuthorizationService authService,
        ILogger<FollowUpService> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    public async Task<FollowUpIndexViewModel> GetFollowUpsAsync(
        ClaimsPrincipal user,
        string? search,
        string? filterTab,
        string? status,
        int page = 1,
        int pageSize = 10)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var query = _context.FollowUps
            .Include(f => f.Beneficiary)
                .ThenInclude(b => b!.Centre)
            .AsNoTracking()
            .AsQueryable();

        // Enforce CentreStaff boundary
        if (!isAdmin && userCentreId.HasValue)
        {
            query = query.Where(f => f.Beneficiary != null && f.Beneficiary.CentreId == userCentreId.Value);
        }

        // Tab Filtering
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
            default:
                break;
        }

        // Specific Status Filter if selected
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            query = query.Where(f => f.Status == status);
        }

        // Search Filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(f =>
                f.Beneficiary != null && (f.Beneficiary.BeneficiaryCode.ToLower().Contains(term) || f.Beneficiary.FullName.ToLower().Contains(term)));
        }

        // Tab Counts
        var baseScoped = _context.FollowUps.AsNoTracking().AsQueryable();
        if (!isAdmin && userCentreId.HasValue)
        {
            baseScoped = baseScoped.Where(f => f.Beneficiary != null && f.Beneficiary.CentreId == userCentreId.Value);
        }

        var totalCount = await baseScoped.CountAsync();
        var pendingCount = await baseScoped.CountAsync(f => f.Status == "Pending" && f.ScheduledDate >= today);
        var overdueCount = await baseScoped.CountAsync(f => f.Status == "Pending" && f.ScheduledDate < today);
        var completedCount = await baseScoped.CountAsync(f => f.Status == "Completed");

        // Sort by ScheduledDate ascending for upcoming, desc for past
        query = tab == "completed" 
            ? query.OrderByDescending(f => f.CompletedDate ?? f.ScheduledDate).ThenByDescending(f => f.FollowUpId)
            : query.OrderBy(f => f.ScheduledDate).ThenBy(f => f.FollowUpId);

        var projected = query.Select(f => new FollowUpListItemViewModel
        {
            FollowUpId = f.FollowUpId,
            BeneficiaryId = f.BeneficiaryId,
            BeneficiaryCode = f.Beneficiary != null ? f.Beneficiary.BeneficiaryCode : string.Empty,
            BeneficiaryName = f.Beneficiary != null ? f.Beneficiary.FullName : string.Empty,
            CentreName = f.Beneficiary != null && f.Beneficiary.Centre != null ? f.Beneficiary.Centre.CentreName : "N/A",
            ScheduledDate = f.ScheduledDate,
            Status = f.Status,
            CompletedDate = f.CompletedDate,
            CreatedAt = f.CreatedAt
        });

        var paginated = await PaginatedList<FollowUpListItemViewModel>.CreateAsync(projected, page, pageSize);

        var statusesList = PredefinedStatuses.Select(st => new SelectListItem
        {
            Value = st,
            Text = st,
            Selected = st == status
        }).ToList();

        return new FollowUpIndexViewModel
        {
            FollowUps = paginated,
            SearchQuery = search,
            FilterTab = tab,
            Status = status ?? "all",
            TotalCount = totalCount,
            PendingCount = pendingCount,
            OverdueCount = overdueCount,
            CompletedCount = completedCount,
            StatusesList = statusesList
        };
    }

    public async Task<FollowUpDetailsViewModel?> GetFollowUpDetailsAsync(int followUpId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var item = await _context.FollowUps
            .Include(f => f.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FollowUpId == followUpId);

        if (item == null)
            return null;

        if (!isAdmin && userCentreId.HasValue && item.Beneficiary?.CentreId != userCentreId.Value)
        {
            _logger.LogWarning("Unauthorized access: User centre {UserCentreId} attempted to view FollowUp {FollowUpId} from centre {CentreId}",
                userCentreId, followUpId, item.Beneficiary?.CentreId);
            return null;
        }

        return new FollowUpDetailsViewModel
        {
            FollowUpId = item.FollowUpId,
            BeneficiaryId = item.BeneficiaryId,
            BeneficiaryCode = item.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = item.Beneficiary?.FullName ?? string.Empty,
            CentreName = item.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = item.Beneficiary?.Centre?.District ?? "N/A",
            StateName = item.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            ScheduledDate = item.ScheduledDate,
            Status = item.Status,
            CompletedDate = item.CompletedDate,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            CanEdit = isAdmin || (userCentreId.HasValue && item.Beneficiary?.CentreId == userCentreId.Value)
        };
    }

    public async Task<CreateFollowUpViewModel> GetCreateViewModelAsync(ClaimsPrincipal user, int? beneficiaryId = null)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var model = new CreateFollowUpViewModel
        {
            ScheduledDate = DateTime.Today.AddDays(14),
            Status = "Pending"
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

    public async Task<CompleteFollowUpViewModel?> GetCompleteViewModelAsync(int followUpId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var item = await _context.FollowUps
            .Include(f => f.Beneficiary)
                .ThenInclude(b => b!.Centre)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FollowUpId == followUpId);

        if (item == null)
            return null;

        if (!isAdmin && userCentreId.HasValue && item.Beneficiary?.CentreId != userCentreId.Value)
        {
            return null;
        }

        return new CompleteFollowUpViewModel
        {
            FollowUpId = item.FollowUpId,
            BeneficiaryId = item.BeneficiaryId,
            BeneficiaryCode = item.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = item.Beneficiary?.FullName ?? string.Empty,
            CentreName = item.Beneficiary?.Centre?.CentreName ?? "N/A",
            ScheduledDate = item.ScheduledDate,
            CompletedDate = item.CompletedDate ?? DateTime.Today,
            Status = item.Status == "Pending" ? "Completed" : item.Status,
            Notes = item.Notes
        };
    }

    public async Task<(bool Success, int FollowUpId, string? Error)> CreateFollowUpAsync(
        CreateFollowUpViewModel model,
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
            _logger.LogWarning("Access Denied: User from centre {UserCentreId} tried to schedule follow-up for beneficiary {BeneficiaryId} belonging to centre {TargetCentreId}",
                userCentreId, beneficiary.BeneficiaryId, beneficiary.CentreId);
            return (false, 0, "Unauthorized: You may only schedule follow-ups for beneficiaries of your assigned centre.");
        }

        var followUp = new FollowUp
        {
            BeneficiaryId = model.BeneficiaryId,
            ScheduledDate = model.ScheduledDate.Date,
            Status = model.Status.Trim(),
            Notes = model.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.FollowUps.Add(followUp);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Scheduled Follow-up {FollowUpId} for Beneficiary {BeneficiaryCode} by user {User}",
            followUp.FollowUpId, beneficiary.BeneficiaryCode, user.Identity?.Name);

        return (true, followUp.FollowUpId, null);
    }

    public async Task<(bool Success, string? Error)> CompleteFollowUpAsync(
        CompleteFollowUpViewModel model,
        ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var item = await _context.FollowUps
            .Include(f => f.Beneficiary)
            .FirstOrDefaultAsync(f => f.FollowUpId == model.FollowUpId);

        if (item == null)
        {
            return (false, "Follow-up record not found.");
        }

        if (!isAdmin && userCentreId.HasValue && item.Beneficiary?.CentreId != userCentreId.Value)
        {
            return (false, "Unauthorized: You cannot update follow-up records belonging to another centre.");
        }

        item.CompletedDate = model.CompletedDate.Date;
        item.Status = model.Status.Trim();
        item.Notes = model.Notes?.Trim();
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Recorded completion for FollowUp {FollowUpId} (Status: {Status}) by user {User}",
            item.FollowUpId, item.Status, user.Identity?.Name);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelFollowUpAsync(int followUpId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var item = await _context.FollowUps
            .Include(f => f.Beneficiary)
            .FirstOrDefaultAsync(f => f.FollowUpId == followUpId);

        if (item == null)
            return (false, "Follow-up not found.");

        if (!isAdmin && userCentreId.HasValue && item.Beneficiary?.CentreId != userCentreId.Value)
            return (false, "Unauthorized action.");

        item.Status = "Cancelled";
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled Follow-up {FollowUpId} by user {User}", followUpId, user.Identity?.Name);
        return (true, null);
    }
}

