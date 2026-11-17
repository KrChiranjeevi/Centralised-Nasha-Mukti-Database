using Microsoft.AspNetCore.Mvc.Rendering;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

/// <summary>
/// Service contract for Beneficiary operations, code generation, and centre-scoped access.
/// </summary>
public interface IBeneficiaryService
{
    Task<BeneficiaryIndexViewModel> GetBeneficiariesAsync(
        string? search, int? centreId, string? status, DateTime? startDate, DateTime? endDate, 
        int page, int pageSize, int? userCentreId, bool isAdmin);

    Task<BeneficiaryDetailsViewModel?> GetBeneficiaryDetailsAsync(int id, int? userCentreId, bool isAdmin);
    Task<Beneficiary?> GetBeneficiaryEntityAsync(int id);
    Task<(bool Success, string? BeneficiaryCode, string? Error)> CreateBeneficiaryAsync(CreateBeneficiaryViewModel model, int? userCentreId, bool isAdmin);
    Task<(bool Success, string? Error)> UpdateBeneficiaryAsync(EditBeneficiaryViewModel model, int? userCentreId, bool isAdmin);
    Task<(bool Success, string? Error)> ToggleBeneficiaryStatusAsync(int id, int? userCentreId, bool isAdmin);
    Task<string> GenerateUniqueBeneficiaryCodeAsync(int centreId);
    Task<List<SelectListItem>> GetActiveCentresSelectListAsync();
}

