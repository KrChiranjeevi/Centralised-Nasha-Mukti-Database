using System.Security.Claims;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

public interface ICounsellingService
{
    Task<CounsellingIndexViewModel> GetSessionsAsync(
        ClaimsPrincipal user,
        string? search,
        int? beneficiaryId,
        string? sessionType,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page = 1,
        int pageSize = 10);

    Task<CounsellingDetailsViewModel?> GetSessionDetailsAsync(int sessionId, ClaimsPrincipal user);

    Task<CreateCounsellingViewModel> GetCreateViewModelAsync(ClaimsPrincipal user, int? beneficiaryId = null);

    Task<EditCounsellingViewModel?> GetEditViewModelAsync(int sessionId, ClaimsPrincipal user);

    Task<(bool Success, int SessionId, string? Error)> CreateSessionAsync(CreateCounsellingViewModel model, ClaimsPrincipal user);

    Task<(bool Success, string? Error)> UpdateSessionAsync(EditCounsellingViewModel model, ClaimsPrincipal user);

    Task<(bool Success, string? Error)> CancelSessionAsync(int sessionId, ClaimsPrincipal user);
}

