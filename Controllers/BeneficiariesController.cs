using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.Data;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Controller for managing Beneficiaries (Registration, Search, Details, Updates, Deactivation).
/// Scoped with Centre-Level Authorization for CentreStaff.
/// </summary>
[Authorize]
public class BeneficiariesController : Controller
{
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ICentreAuthorizationService _authService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BeneficiariesController> _logger;

    public BeneficiariesController(
        IBeneficiaryService beneficiaryService,
        ICentreAuthorizationService authService,
        ApplicationDbContext context,
        ILogger<BeneficiariesController> logger)
    {
        _beneficiaryService = beneficiaryService;
        _authService = authService;
        _context = context;
        _logger = logger;
    }

    // ── GET: /Beneficiaries ──────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search, int? centreId, string? status, DateTime? startDate, DateTime? endDate, int page = 1)
    {
        ViewData["Title"] = "Beneficiaries";

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        const int pageSize = 10;
        var viewModel = await _beneficiaryService.GetBeneficiariesAsync(
            search, centreId, status, startDate, endDate, page, pageSize, userCentreId, isAdmin);

        return View(viewModel);
    }

    // ── GET: /Beneficiaries/Details/5 ────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        // Security check: CentreStaff can only view beneficiaries at their centre
        if (!isAdmin && !await _authService.CanAccessBeneficiaryAsync(User, id))
        {
            _logger.LogWarning("Access Denied: User {User} attempted to view foreign Beneficiary {Id}",
                User.Identity?.Name, id);
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        var details = await _beneficiaryService.GetBeneficiaryDetailsAsync(id, userCentreId, isAdmin);
        if (details == null)
        {
            return View("~/Views/Shared/NotFound.cshtml");
        }

        ViewData["Title"] = $"{details.FullName} ({details.BeneficiaryCode})";
        ViewData["BreadcrumbParent"] = "Beneficiaries";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Index");

        return View(details);
    }

    // ── GET: /Beneficiaries/Create ───────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Register Beneficiary";
        ViewData["BreadcrumbParent"] = "Beneficiaries";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Index");

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        string? centreName = null;
        if (!isAdmin && userCentreId.HasValue)
        {
            var centre = await _context.Centres.FindAsync(userCentreId.Value);
            centreName = centre?.CentreName;
        }

        var viewModel = new CreateBeneficiaryViewModel
        {
            IsAdmin = isAdmin,
            CentreId = userCentreId ?? 0,
            AssignedCentreName = centreName,
            CentresList = await _beneficiaryService.GetActiveCentresSelectListAsync(),
            RegistrationDate = DateTime.Today
        };

        return View(viewModel);
    }

    // ── POST: /Beneficiaries/Create ──────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBeneficiaryViewModel model)
    {
        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        model.IsAdmin = isAdmin;

        // Security rule: If CentreStaff, enforce user's CentreId unconditionally
        if (!isAdmin)
        {
            if (!userCentreId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Your account is not assigned to an active centre.");
            }
            else
            {
                model.CentreId = userCentreId.Value;
            }
        }

        if (!ModelState.IsValid)
        {
            model.CentresList = await _beneficiaryService.GetActiveCentresSelectListAsync();
            return View(model);
        }

        var (success, code, error) = await _beneficiaryService.CreateBeneficiaryAsync(model, userCentreId, isAdmin);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to register beneficiary.");
            model.CentresList = await _beneficiaryService.GetActiveCentresSelectListAsync();
            return View(model);
        }

        TempData["Success"] = $"Beneficiary '{model.FullName}' registered successfully with ID: {code}";
        return RedirectToAction(nameof(Index));
    }

    // ── GET: /Beneficiaries/Edit/5 ────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        if (!isAdmin && !await _authService.CanAccessBeneficiaryAsync(User, id))
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        var beneficiary = await _beneficiaryService.GetBeneficiaryEntityAsync(id);
        if (beneficiary == null)
        {
            return View("~/Views/Shared/NotFound.cshtml");
        }

        ViewData["Title"] = $"Edit {beneficiary.BeneficiaryCode}";
        ViewData["BreadcrumbParent"] = "Beneficiaries";
        ViewData["BreadcrumbParentUrl"] = Url.Action("Index");

        var viewModel = new EditBeneficiaryViewModel
        {
            BeneficiaryId = beneficiary.BeneficiaryId,
            BeneficiaryCode = beneficiary.BeneficiaryCode,
            FullName = beneficiary.FullName,
            CentreId = beneficiary.CentreId,
            RegistrationDate = beneficiary.RegistrationDate,
            DateOfBirth = beneficiary.DateOfBirth,
            Gender = beneficiary.Gender,
            ContactNumber = beneficiary.ContactNumber,
            Status = beneficiary.Status,
            IsAdmin = isAdmin,
            CentresList = await _beneficiaryService.GetActiveCentresSelectListAsync()
        };

        return View(viewModel);
    }

    // ── POST: /Beneficiaries/Edit/5 ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditBeneficiaryViewModel model)
    {
        if (id != model.BeneficiaryId)
        {
            return BadRequest();
        }

        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        if (!isAdmin && !await _authService.CanAccessBeneficiaryAsync(User, id))
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        model.IsAdmin = isAdmin;

        if (!ModelState.IsValid)
        {
            model.CentresList = await _beneficiaryService.GetActiveCentresSelectListAsync();
            return View(model);
        }

        var (success, error) = await _beneficiaryService.UpdateBeneficiaryAsync(model, userCentreId, isAdmin);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update beneficiary.");
            model.CentresList = await _beneficiaryService.GetActiveCentresSelectListAsync();
            return View(model);
        }

        TempData["Success"] = $"Beneficiary '{model.FullName}' ({model.BeneficiaryCode}) updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.BeneficiaryId });
    }

    // ── POST: /Beneficiaries/ToggleStatus/5 ───────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var isAdmin = await _authService.IsAdminAsync(User);
        var userCentreId = await _authService.GetUserCentreIdAsync(User);

        var (success, error) = await _beneficiaryService.ToggleBeneficiaryStatusAsync(id, userCentreId, isAdmin);
        if (!success)
        {
            TempData["Error"] = error ?? "Failed to update beneficiary status.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Beneficiary status updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
