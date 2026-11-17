using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

public class CounsellingService : ICounsellingService
{
    private readonly ApplicationDbContext _context;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<CounsellingService> _logger;

    private static readonly string[] PredefinedTypes =
    {
        "Individual Counselling",
        "Group Counselling",
        "Family Therapy",
        "Relapse Prevention",
        "Crisis Intervention",
        "Motivational Interviewing"
    };

    private static readonly string[] PredefinedStatuses =
    {
        "Completed",
        "Scheduled",
        "Cancelled",
        "No-Show"
    };

    public CounsellingService(
        ApplicationDbContext context,
        ICentreAuthorizationService authService,
        ILogger<CounsellingService> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    public async Task<CounsellingIndexViewModel> GetSessionsAsync(
        ClaimsPrincipal user,
        string? search,
        int? beneficiaryId,
        string? sessionType,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page = 1,
        int pageSize = 10)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

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

        // Beneficiary Filter
        string? beneficiaryName = null;
        if (beneficiaryId.HasValue)
        {
            query = query.Where(s => s.BeneficiaryId == beneficiaryId.Value);
            beneficiaryName = await _context.Beneficiaries
                .Where(b => b.BeneficiaryId == beneficiaryId.Value)
                .Select(b => $"{b.FullName} ({b.BeneficiaryCode})")
                .FirstOrDefaultAsync();
        }

        // Search Filter (Beneficiary Code, Name, or Counsellor)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                (s.Beneficiary != null && (s.Beneficiary.BeneficiaryCode.ToLower().Contains(term) || s.Beneficiary.FullName.ToLower().Contains(term))) ||
                (s.CounsellorName != null && s.CounsellorName.ToLower().Contains(term)));
        }

        // Session Type Filter
        if (!string.IsNullOrWhiteSpace(sessionType) && sessionType != "all")
        {
            query = query.Where(s => s.SessionType == sessionType);
        }

        // Status Filter
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            query = query.Where(s => s.SessionStatus == status);
        }

        // Date Range Filter
        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            query = query.Where(s => s.SessionDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date;
            query = query.Where(s => s.SessionDate <= to);
        }

        // Counts for quick badges
        var baseScoped = _context.CounsellingSessions.AsNoTracking().AsQueryable();
        if (!isAdmin && userCentreId.HasValue)
        {
            baseScoped = baseScoped.Where(s => s.Beneficiary != null && s.Beneficiary.CentreId == userCentreId.Value);
        }

        var totalSessionsCount = await baseScoped.CountAsync();
        var completedCount = await baseScoped.CountAsync(s => s.SessionStatus == "Completed");
        var scheduledCount = await baseScoped.CountAsync(s => s.SessionStatus == "Scheduled");

        // Order by session date desc
        query = query.OrderByDescending(s => s.SessionDate).ThenByDescending(s => s.SessionId);

        var projected = query.Select(s => new CounsellingSessionListItemViewModel
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
        });

        var paginated = await PaginatedList<CounsellingSessionListItemViewModel>.CreateAsync(projected, page, pageSize);

        var typesList = PredefinedTypes.Select(t => new SelectListItem
        {
            Value = t,
            Text = t,
            Selected = t == sessionType
        }).ToList();

        var statusesList = PredefinedStatuses.Select(st => new SelectListItem
        {
            Value = st,
            Text = st,
            Selected = st == status
        }).ToList();

        return new CounsellingIndexViewModel
        {
            Sessions = paginated,
            SearchQuery = search,
            BeneficiaryId = beneficiaryId,
            BeneficiaryName = beneficiaryName,
            SessionType = sessionType ?? "all",
            Status = status ?? "all",
            FromDate = fromDate,
            ToDate = toDate,
            TotalSessionsCount = totalSessionsCount,
            CompletedCount = completedCount,
            ScheduledCount = scheduledCount,
            TypesList = typesList,
            StatusesList = statusesList
        };
    }

    public async Task<CounsellingDetailsViewModel?> GetSessionDetailsAsync(int sessionId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var session = await _context.CounsellingSessions
            .Include(s => s.Beneficiary)
                .ThenInclude(b => b!.Centre)
                    .ThenInclude(c => c!.State)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
            return null;

        // Authorize centre access
        if (!isAdmin && userCentreId.HasValue && session.Beneficiary?.CentreId != userCentreId.Value)
        {
            _logger.LogWarning("Unauthorized access: User centre {UserCentreId} attempted to read Counselling {SessionId} from centre {CentreId}",
                userCentreId, sessionId, session.Beneficiary?.CentreId);
            return null;
        }

        return new CounsellingDetailsViewModel
        {
            SessionId = session.SessionId,
            BeneficiaryId = session.BeneficiaryId,
            BeneficiaryCode = session.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = session.Beneficiary?.FullName ?? string.Empty,
            CentreName = session.Beneficiary?.Centre?.CentreName ?? "N/A",
            District = session.Beneficiary?.Centre?.District ?? "N/A",
            StateName = session.Beneficiary?.Centre?.State?.StateName ?? "N/A",
            SessionDate = session.SessionDate,
            SessionType = session.SessionType,
            CounsellorName = session.CounsellorName,
            SessionStatus = session.SessionStatus,
            Notes = session.Notes,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            CanEdit = isAdmin || (userCentreId.HasValue && session.Beneficiary?.CentreId == userCentreId.Value)
        };
    }

    public async Task<CreateCounsellingViewModel> GetCreateViewModelAsync(ClaimsPrincipal user, int? beneficiaryId = null)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var model = new CreateCounsellingViewModel
        {
            SessionDate = DateTime.Today,
            SessionType = "Individual Counselling",
            SessionStatus = "Completed",
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

    public async Task<EditCounsellingViewModel?> GetEditViewModelAsync(int sessionId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var session = await _context.CounsellingSessions
            .Include(s => s.Beneficiary)
                .ThenInclude(b => b!.Centre)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
            return null;

        if (!isAdmin && userCentreId.HasValue && session.Beneficiary?.CentreId != userCentreId.Value)
        {
            return null;
        }

        return new EditCounsellingViewModel
        {
            SessionId = session.SessionId,
            BeneficiaryId = session.BeneficiaryId,
            BeneficiaryCode = session.Beneficiary?.BeneficiaryCode ?? string.Empty,
            BeneficiaryName = session.Beneficiary?.FullName ?? string.Empty,
            CentreName = session.Beneficiary?.Centre?.CentreName ?? "N/A",
            SessionDate = session.SessionDate,
            SessionType = session.SessionType,
            CounsellorName = session.CounsellorName,
            SessionStatus = session.SessionStatus,
            Notes = session.Notes,
            TypesList = PredefinedTypes.Select(t => new SelectListItem
            {
                Value = t,
                Text = t,
                Selected = t == session.SessionType
            }).ToList(),
            StatusesList = PredefinedStatuses.Select(st => new SelectListItem
            {
                Value = st,
                Text = st,
                Selected = st == session.SessionStatus
            }).ToList()
        };
    }

    public async Task<(bool Success, int SessionId, string? Error)> CreateSessionAsync(
        CreateCounsellingViewModel model,
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
            _logger.LogWarning("Access Denied: User from centre {UserCentreId} tried to record counselling for beneficiary {BeneficiaryId} belonging to centre {TargetCentreId}",
                userCentreId, beneficiary.BeneficiaryId, beneficiary.CentreId);
            return (false, 0, "Unauthorized: You may only record counselling sessions for beneficiaries of your assigned centre.");
        }

        var session = new CounsellingSession
        {
            BeneficiaryId = model.BeneficiaryId,
            SessionDate = model.SessionDate.Date,
            SessionType = model.SessionType.Trim(),
            CounsellorName = model.CounsellorName?.Trim(),
            SessionStatus = model.SessionStatus.Trim(),
            Notes = model.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.CounsellingSessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded Counselling Session {SessionId} for Beneficiary {BeneficiaryCode} by user {User}",
            session.SessionId, beneficiary.BeneficiaryCode, user.Identity?.Name);

        return (true, session.SessionId, null);
    }

    public async Task<(bool Success, string? Error)> UpdateSessionAsync(
        EditCounsellingViewModel model,
        ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var session = await _context.CounsellingSessions
            .Include(s => s.Beneficiary)
            .FirstOrDefaultAsync(s => s.SessionId == model.SessionId);

        if (session == null)
        {
            return (false, "Counselling session not found.");
        }

        if (!isAdmin && userCentreId.HasValue && session.Beneficiary?.CentreId != userCentreId.Value)
        {
            return (false, "Unauthorized: You cannot edit sessions belonging to another centre.");
        }

        session.SessionDate = model.SessionDate.Date;
        session.SessionType = model.SessionType.Trim();
        session.CounsellorName = model.CounsellorName?.Trim();
        session.SessionStatus = model.SessionStatus.Trim();
        session.Notes = model.Notes?.Trim();
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated Counselling Session {SessionId} by user {User}", session.SessionId, user.Identity?.Name);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelSessionAsync(int sessionId, ClaimsPrincipal user)
    {
        var isAdmin = await _authService.IsAdminAsync(user);
        var userCentreId = await _authService.GetUserCentreIdAsync(user);

        var session = await _context.CounsellingSessions
            .Include(s => s.Beneficiary)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
            return (false, "Session not found.");

        if (!isAdmin && userCentreId.HasValue && session.Beneficiary?.CentreId != userCentreId.Value)
            return (false, "Unauthorized action.");

        session.SessionStatus = "Cancelled";
        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled Counselling Session {SessionId} by user {User}", sessionId, user.Identity?.Name);
        return (true, null);
    }
}

