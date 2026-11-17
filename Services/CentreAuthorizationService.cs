using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;

namespace NashaMuktiDB.Services;

/// <summary>
/// Implements Centre-Level authorization checks using ASP.NET Core Identity and EF Core.
/// Guarantees that CentreStaff can only access data belonging to their assigned facility.
/// </summary>
public class CentreAuthorizationService : ICentreAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public CentreAuthorizationService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return null;

        return await _userManager.GetUserAsync(principal);
    }

    public async Task<bool> IsAdminAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return false;

        return principal.IsInRole("Admin");
    }

    public async Task<int?> GetUserCentreIdAsync(ClaimsPrincipal principal)
    {
        var user = await GetCurrentUserAsync(principal);
        return user?.CentreId;
    }

    public async Task<bool> CanAccessCentreAsync(ClaimsPrincipal principal, int centreId)
    {
        if (await IsAdminAsync(principal))
            return true;

        var userCentreId = await GetUserCentreIdAsync(principal);
        return userCentreId.HasValue && userCentreId.Value == centreId;
    }

    public async Task<bool> CanAccessBeneficiaryAsync(ClaimsPrincipal principal, int beneficiaryId)
    {
        if (await IsAdminAsync(principal))
            return true;

        var userCentreId = await GetUserCentreIdAsync(principal);
        if (!userCentreId.HasValue)
            return false;

        var beneficiaryCentreId = await _context.Beneficiaries
            .Where(b => b.BeneficiaryId == beneficiaryId)
            .Select(b => b.CentreId)
            .FirstOrDefaultAsync();

        return beneficiaryCentreId == userCentreId.Value;
    }
}

