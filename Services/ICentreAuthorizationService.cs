using System.Security.Claims;
using NashaMuktiDB.Models;

namespace NashaMuktiDB.Services;

/// <summary>
/// Service contract for enforcing Centre-Level authorization and role restrictions.
/// Ensures CentreStaff cannot access or tamper with data belonging to other facilities.
/// </summary>
public interface ICentreAuthorizationService
{
    Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal principal);
    Task<int?> GetUserCentreIdAsync(ClaimsPrincipal principal);
    Task<bool> IsAdminAsync(ClaimsPrincipal principal);
    Task<bool> CanAccessCentreAsync(ClaimsPrincipal principal, int centreId);
    Task<bool> CanAccessBeneficiaryAsync(ClaimsPrincipal principal, int beneficiaryId);
}

