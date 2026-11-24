using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Administrative Controller for user account management, registration request reviews,
/// and role-based administration. Restricted strictly to authenticated users with the 'Admin' role.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    // ── GET: /Admin/Users ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        ViewData["Title"] = "User Management";

        var users = await _userManager.Users
            .Include(u => u.Centre)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var userList = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new UserListViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Unassigned",
                CentreId = user.CentreId,
                CentreName = user.Centre?.CentreName ?? "National / All Centres",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        ViewBag.PendingRequestsCount = await _context.AccountRequests
            .CountAsync(r => r.Status == nameof(AccountRequestStatus.Pending));

        return View(userList);
    }

    // ── GET: /Admin/AccountRequests ──────────────────────────────
    [HttpGet]
    public async Task<IActionResult> AccountRequests(string? status = "Pending")
    {
        ViewData["Title"] = "Registration Access Requests";

        var query = _context.AccountRequests
            .Include(r => r.RequestedCentre)
            .Include(r => r.ReviewedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => r.Status == status);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AccountRequestItemViewModel
            {
                AccountRequestId = r.AccountRequestId,
                FullName = r.FullName,
                Email = r.Email,
                ContactNumber = r.ContactNumber,
                RequestedCentreId = r.RequestedCentreId,
                RequestedCentreName = r.RequestedCentre != null ? r.RequestedCentre.CentreName : "N/A",
                ReasonForAccess = r.ReasonForAccess,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ReviewedAt = r.ReviewedAt,
                ReviewedByName = r.ReviewedByUser != null ? r.ReviewedByUser.FullName : null,
                AdminNotes = r.AdminNotes,
                ApprovedUserId = r.ApprovedUserId
            })
            .ToListAsync();

        ViewBag.CurrentStatusFilter = status;
        ViewBag.PendingCount = await _context.AccountRequests.CountAsync(r => r.Status == nameof(AccountRequestStatus.Pending));
        ViewBag.ApprovedCount = await _context.AccountRequests.CountAsync(r => r.Status == nameof(AccountRequestStatus.Approved));
        ViewBag.RejectedCount = await _context.AccountRequests.CountAsync(r => r.Status == nameof(AccountRequestStatus.Rejected));
        ViewBag.TotalCount = await _context.AccountRequests.CountAsync();

        return View(requests);
    }

    // ── GET: /Admin/ReviewRequest/{id} ───────────────────────────
    [HttpGet]
    public async Task<IActionResult> ReviewRequest(int id)
    {
        ViewData["Title"] = "Review Access Request";

        var request = await _context.AccountRequests
            .Include(r => r.RequestedCentre)
            .FirstOrDefaultAsync(r => r.AccountRequestId == id);

        if (request == null)
        {
            TempData["Error"] = "Account request not found.";
            return RedirectToAction(nameof(AccountRequests));
        }

        if (request.Status != nameof(AccountRequestStatus.Pending))
        {
            TempData["Error"] = $"This request has already been processed with status '{request.Status}'.";
            return RedirectToAction(nameof(AccountRequests));
        }

        await PopulateCentresDropDownAsync(request.RequestedCentreId);

        var viewModel = new ApproveAccountRequestViewModel
        {
            AccountRequestId = request.AccountRequestId,
            FullName = request.FullName,
            Email = request.Email,
            ContactNumber = request.ContactNumber,
            RequestedCentreId = request.RequestedCentreId,
            RequestedCentreName = request.RequestedCentre?.CentreName ?? "Unknown",
            ReasonForAccess = request.ReasonForAccess,
            CreatedAt = request.CreatedAt,
            AssignedRole = DatabaseSeeder.RoleCentreStaff,
            AssignedCentreId = request.RequestedCentreId,
            TemporaryPassword = "Temp@NMD2026!",
            ConfirmPassword = "Temp@NMD2026!",
            RequirePasswordChange = true
        };

        return View(viewModel);
    }

    // ── POST: /Admin/ApproveRequest ──────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRequest(ApproveAccountRequestViewModel model)
    {
        var request = await _context.AccountRequests
            .Include(r => r.RequestedCentre)
            .FirstOrDefaultAsync(r => r.AccountRequestId == model.AccountRequestId);

        if (request == null)
        {
            TempData["Error"] = "Account request not found.";
            return RedirectToAction(nameof(AccountRequests));
        }

        if (request.Status != nameof(AccountRequestStatus.Pending))
        {
            TempData["Error"] = $"This request has already been reviewed (Status: {request.Status}).";
            return RedirectToAction(nameof(AccountRequests));
        }

        // Server-side role and centre validation
        if (model.AssignedRole == DatabaseSeeder.RoleCentreStaff && !model.AssignedCentreId.HasValue)
        {
            ModelState.AddModelError(nameof(model.AssignedCentreId), "Centre Staff must be assigned to an active Nasha Mukti Kendra.");
        }

        if (model.AssignedRole == DatabaseSeeder.RoleAdmin)
        {
            model.AssignedCentreId = null; // National Admins have nationwide scope
        }

        // Verify that centre exists and is active if assigned
        if (model.AssignedCentreId.HasValue)
        {
            var centre = await _context.Centres.FirstOrDefaultAsync(c => c.CentreId == model.AssignedCentreId.Value && c.IsActive);
            if (centre == null)
            {
                ModelState.AddModelError(nameof(model.AssignedCentreId), "The selected centre is not valid or active.");
            }
        }

        // Ensure user account does not already exist
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, $"A user account with email '{request.Email}' already exists in the system.");
        }

        if (!ModelState.IsValid)
        {
            model.FullName = request.FullName;
            model.Email = request.Email;
            model.ContactNumber = request.ContactNumber;
            model.RequestedCentreName = request.RequestedCentre?.CentreName ?? "Unknown";
            model.ReasonForAccess = request.ReasonForAccess;
            model.CreatedAt = request.CreatedAt;
            await PopulateCentresDropDownAsync(model.AssignedCentreId);
            return View(nameof(ReviewRequest), model);
        }

        // Create the new Identity user
        var newUser = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.ContactNumber,
            CentreId = model.AssignedCentreId,
            IsActive = true,
            EmailConfirmed = true,
            MustChangePassword = model.RequirePasswordChange,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(newUser, model.TemporaryPassword);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            model.FullName = request.FullName;
            model.Email = request.Email;
            model.ContactNumber = request.ContactNumber;
            model.RequestedCentreName = request.RequestedCentre?.CentreName ?? "Unknown";
            model.ReasonForAccess = request.ReasonForAccess;
            model.CreatedAt = request.CreatedAt;
            await PopulateCentresDropDownAsync(model.AssignedCentreId);
            return View(nameof(ReviewRequest), model);
        }

        // Assign role
        await _userManager.AddToRoleAsync(newUser, model.AssignedRole);

        // Update request record
        var currentAdmin = await _userManager.GetUserAsync(User);
        request.Status = nameof(AccountRequestStatus.Approved);
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedByUserId = currentAdmin?.Id;
        request.ApprovedUserId = newUser.Id;
        request.AdminNotes = model.AdminNotes?.Trim();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminUser} approved access request #{RequestId} for {Email}. User ID: {UserId}, Role: {Role}, Centre: {CentreId}",
            User.Identity?.Name, request.AccountRequestId, newUser.Email, newUser.Id, model.AssignedRole, model.AssignedCentreId);

        TempData["Success"] = $"Access request for '{newUser.FullName}' has been approved successfully. Account '{newUser.Email}' created with role '{model.AssignedRole}'.";
        return RedirectToAction(nameof(AccountRequests));
    }

    // ── POST: /Admin/RejectRequest ──────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequest(int id, string? rejectionReason)
    {
        var request = await _context.AccountRequests.FindAsync(id);
        if (request == null)
        {
            TempData["Error"] = "Account request not found.";
            return RedirectToAction(nameof(AccountRequests));
        }

        if (request.Status != nameof(AccountRequestStatus.Pending))
        {
            TempData["Error"] = $"This request has already been processed with status '{request.Status}'.";
            return RedirectToAction(nameof(AccountRequests));
        }

        var currentAdmin = await _userManager.GetUserAsync(User);
        request.Status = nameof(AccountRequestStatus.Rejected);
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedByUserId = currentAdmin?.Id;
        request.AdminNotes = string.IsNullOrWhiteSpace(rejectionReason) ? "Declined by administrator." : rejectionReason.Trim();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminUser} rejected access request #{RequestId} for {Email}. Reason: {Reason}",
            User.Identity?.Name, id, request.Email, request.AdminNotes);

        TempData["Success"] = $"Access request for '{request.FullName}' has been declined.";
        return RedirectToAction(nameof(AccountRequests));
    }

    // ── GET: /Admin/CreateUser ───────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        ViewData["Title"] = "Add Authorized User";
        await PopulateCentresDropDownAsync();
        return View(new CreateUserViewModel());
    }

    // ── POST: /Admin/CreateUser ──────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (model.Role == DatabaseSeeder.RoleCentreStaff && !model.CentreId.HasValue)
        {
            ModelState.AddModelError("CentreId", "Centre staff must be assigned to an active Nasha Mukti Kendra.");
        }

        if (model.Role == DatabaseSeeder.RoleAdmin)
        {
            model.CentreId = null; // Admins are not tied to a specific centre
        }

        if (!ModelState.IsValid)
        {
            await PopulateCentresDropDownAsync(model.CentreId);
            return View(model);
        }

        // Verify if email is already registered
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "A user account with this email address already exists.");
            await PopulateCentresDropDownAsync(model.CentreId);
            return View(model);
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            CentreId = model.CentreId,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await PopulateCentresDropDownAsync(model.CentreId);
            return View(model);
        }

        // Assign selected role
        await _userManager.AddToRoleAsync(user, model.Role);
        _logger.LogInformation("Admin {AdminUser} created new user {NewUser} with role {Role}",
            User.Identity?.Name, user.Email, model.Role);

        TempData["Success"] = $"User '{model.FullName}' created successfully with role '{model.Role}'.";
        return RedirectToAction(nameof(Users));
    }

    // ── POST: /Admin/ToggleStatus ────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(Users));
        }

        // Prevent self-deactivation
        if (user.UserName == User.Identity?.Name)
        {
            TempData["Error"] = "You cannot deactivate your own administrative account.";
            return RedirectToAction(nameof(Users));
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateSecurityStampAsync(user);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Admin {AdminUser} toggled user {TargetUser} (ID: {UserId}) status to {Status}",
            User.Identity?.Name, user.Email, user.Id, user.IsActive ? "Active" : "Inactive");

        TempData["Success"] = $"User '{user.FullName}' status changed to {(user.IsActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Users));
    }

    // ── GET: /Admin/AuditLog ─────────────────────────────────────
    [HttpGet]
    public IActionResult AuditLog()
    {
        ViewData["Title"] = "System Audit Log";
        return View();
    }

    private async Task PopulateCentresDropDownAsync(int? selectedCentreId = null)
    {
        var centres = await _context.Centres
            .Where(c => c.IsActive)
            .OrderBy(c => c.CentreName)
            .Select(c => new
            {
                c.CentreId,
                DisplayName = $"{c.CentreName} ({c.District})"
            })
            .ToListAsync();

        ViewBag.CentresList = new SelectList(centres, "CentreId", "DisplayName", selectedCentreId);
    }
}
