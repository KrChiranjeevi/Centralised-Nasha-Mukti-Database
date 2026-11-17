using System.Security.Claims;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

public interface IInterventionService
{
    Task<InterventionIndexViewModel> GetInterventionsAsync(
        ClaimsPrincipal user,
        string? search,
        int? beneficiaryId,
        string? interventionType,
        string? status,
        int page = 1,
        int pageSize = 10);

    Task<InterventionDetailsViewModel?> GetInterventionDetailsAsync(int interventionId, ClaimsPrincipal user);

    Task<CreateInterventionViewModel> GetCreateViewModelAsync(ClaimsPrincipal user, int? beneficiaryId = null);

    Task<EditInterventionViewModel?> GetEditViewModelAsync(int interventionId, ClaimsPrincipal user);

    Task<(bool Success, int InterventionId, string? Error)> CreateInterventionAsync(CreateInterventionViewModel model, ClaimsPrincipal user);

    Task<(bool Success, string? Error)> UpdateInterventionAsync(EditInterventionViewModel model, ClaimsPrincipal user);

    Task<(bool Success, string? Error)> CancelInterventionAsync(int interventionId, ClaimsPrincipal user);
}

