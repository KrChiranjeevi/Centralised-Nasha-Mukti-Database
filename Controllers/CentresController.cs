using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Controller for managing Nasha Mukti Centres.
/// National Administrators have full CRUD permissions.
/// Centre Staff are strictly restricted to their assigned centre.
/// </summary>
[Authorize]
public class CentresController : Controller
{
    private readonly ICentreService _centreService;
    private readonly ICentreAuthorizationService _authService;
    private readonly ILogger<CentresController> _logger;

    public CentresController(
        ICentreService centreService,
        ICentreAuthorizationService authService,
        ILogger<CentresController> logger)
    {
        _centreService = centreService;
        _authService = authService;
        _logger = logger;
    }

    // ── GET: /Centres ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? stateId, string? status, int page = 1)
    {
        ViewData["Title"] = "Centres Directory";

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        const int pageSize = 10;
        var viewModel = await _centreService.GetCentresAsync(
            search, stateId, status, page, pageSize, userCentreId, isAdmin);

        return View(viewModel);
    }

    // ── GET: /Centres/Details/5 ──────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        // Security: Disallow CentreStaff from viewing other centres
        if (!isAdmin && (!userCentreId.HasValue || userCentreId.Value != id))
        {
            _logger.LogWarning("Access denied: User {User} attempted to view Centre {CentreId}",
                User.Identity?.Name, id);
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        var centre = await _centreService.GetCentreDetailsAsync(id, userCentreId, isAdmin);
        if (centre == null)
        {
            return View("~/Views/Shared/NotFound.cshtml");
        }

        ViewData["Title"] = centre.CentreName;
        ViewData["BreadcrumbParent"] = "Centres";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Index");

        return View(centre);
    }

    // ── GET: /Centres/Create ─────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add New Centre";
        ViewData["BreadcrumbParent"] = "Centres";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Index");

        var viewModel = new CreateCentreViewModel
        {
            StatesList = await _centreService.GetStatesSelectListAsync()
        };

        return View(viewModel);
    }

    // ── POST: /Centres/Create ────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCentreViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.StatesList = await _centreService.GetStatesSelectListAsync();
            return View(model);
        }

        var success = await _centreService.CreateCentreAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Unable to register centre. Please verify the selected State exists.");
            model.StatesList = await _centreService.GetStatesSelectListAsync();
            return View(model);
        }

        TempData["Success"] = $"Nasha Mukti Centre '{model.CentreName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET: /Centres/Edit/5 ──────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var centre = await _centreService.GetCentreEntityAsync(id);
        if (centre == null)
        {
            return View("~/Views/Shared/NotFound.cshtml");
        }

        ViewData["Title"] = $"Edit: {centre.CentreName}";
        ViewData["BreadcrumbParent"] = "Centres";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Index");

        var viewModel = new EditCentreViewModel
        {
            CentreId = centre.CentreId,
            CentreName = centre.CentreName,
            StateId = centre.StateId,
            District = centre.District,
            Address = centre.Address,
            ContactNumber = centre.ContactNumber,
            Email = centre.Email,
            IsActive = centre.IsActive,
            StatesList = await _centreService.GetStatesSelectListAsync()
        };

        return View(viewModel);
    }

    // ── POST: /Centres/Edit/5 ─────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCentreViewModel model)
    {
        if (id != model.CentreId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.StatesList = await _centreService.GetStatesSelectListAsync();
            return View(model);
        }

        var success = await _centreService.UpdateCentreAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to update centre details. Please check form values.");
            model.StatesList = await _centreService.GetStatesSelectListAsync();
            return View(model);
        }

        TempData["Success"] = $"Centre '{model.CentreName}' updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.CentreId });
    }

    // ── POST: /Centres/ToggleStatus/5 ─────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var success = await _centreService.ToggleCentreStatusAsync(id);
        if (!success)
        {
            TempData["Error"] = "Centre not found.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Centre status updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
