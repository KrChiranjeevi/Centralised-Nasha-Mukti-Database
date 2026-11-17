using System.Security.Claims;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Services;

public interface IReportService
{
    Task<ReportsIndexViewModel> GetReportsAsync(
        ClaimsPrincipal user,
        ReportsFilterViewModel filter,
        int page = 1,
        int pageSize = 15);

    Task<byte[]> ExportSummaryCsvAsync(ClaimsPrincipal user, ReportsFilterViewModel filter);

    Task<byte[]> ExportBeneficiariesCsvAsync(ClaimsPrincipal user, ReportsFilterViewModel filter);

    Task<ReportsFilterViewModel> PopulateFilterOptionsAsync(ClaimsPrincipal user, ReportsFilterViewModel filter);
}

