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
/// Administrative Controller for user account management and role-based administration.
/// Restricted strictly to authenticated users with the 'Admin' role.
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

        return View(userList);
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
            await PopulateCentresDropDownAsync();
            return View(model);
        }

        // Verify if email is already registered
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "A user account with this email address already exists.");
            await PopulateCentresDropDownAsync();
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
            await PopulateCentresDropDownAsync();
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
        await _userManager.UpdateAsync(user);

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

    private async Task PopulateCentresDropDownAsync()
    {
        var centres = await _context.Centres
            .Where(c => c.IsActive)
            .OrderBy(c => c.CentreName)
            .Select(c => new { c.CentreId, c.CentreName, c.District })
            .ToListAsync();

        ViewBag.CentresList = new SelectList(centres, "CentreId", "CentreName");
    }
}
