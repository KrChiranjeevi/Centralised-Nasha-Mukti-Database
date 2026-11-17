using Microsoft.AspNetCore.Mvc.Rendering;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

/// <summary>
/// Service contract for Centre operations, queries, and business validations.
/// </summary>
public interface ICentreService
{
    Task<CentreIndexViewModel> GetCentresAsync(string? search, int? stateId, string? status, int page, int pageSize, int? userCentreId, bool isAdmin);
    Task<CentreDetailsViewModel?> GetCentreDetailsAsync(int id, int? userCentreId, bool isAdmin);
    Task<Centre?> GetCentreEntityAsync(int id);
    Task<bool> CreateCentreAsync(CreateCentreViewModel model);
    Task<bool> UpdateCentreAsync(EditCentreViewModel model);
    Task<bool> ToggleCentreStatusAsync(int id);
    Task<List<SelectListItem>> GetStatesSelectListAsync();
    Task<int> GetActiveCentresCountAsync();
    Task<int> GetTotalCentresCountAsync();
}

