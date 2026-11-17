using System.Security.Claims;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

public interface IFollowUpService
{
    Task<FollowUpIndexViewModel> GetFollowUpsAsync(
        ClaimsPrincipal user,
        string? search,
        string? filterTab,
        string? status,
        int page = 1,
        int pageSize = 10);

    Task<FollowUpDetailsViewModel?> GetFollowUpDetailsAsync(int followUpId, ClaimsPrincipal user);

    Task<CreateFollowUpViewModel> GetCreateViewModelAsync(ClaimsPrincipal user, int? beneficiaryId = null);

    Task<CompleteFollowUpViewModel?> GetCompleteViewModelAsync(int followUpId, ClaimsPrincipal user);

    Task<(bool Success, int FollowUpId, string? Error)> CreateFollowUpAsync(CreateFollowUpViewModel model, ClaimsPrincipal user);

    Task<(bool Success, string? Error)> CompleteFollowUpAsync(CompleteFollowUpViewModel model, ClaimsPrincipal user);

    Task<(bool Success, string? Error)> CancelFollowUpAsync(int followUpId, ClaimsPrincipal user);
}

