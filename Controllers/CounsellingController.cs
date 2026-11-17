using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class CounsellingController : Controller
{
    private readonly ICounsellingService _counsellingService;
    private readonly ILogger<CounsellingController> _logger;

    public CounsellingController(
        ICounsellingService counsellingService,
        ILogger<CounsellingController> logger)
    {
        _counsellingService = counsellingService;
        _logger = logger;
    }

    // GET: /Counselling
    public async Task<IActionResult> Index(
        string? search,
        int? beneficiaryId,
        string? sessionType,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page = 1)
    {
        ViewData["Title"] = "Counselling Sessions";
        var vm = await _counsellingService.GetSessionsAsync(
            User, search, beneficiaryId, sessionType, status, fromDate, toDate, page, pageSize: 10);
        return View(vm);
    }

    // GET: /Counselling/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _counsellingService.GetSessionDetailsAsync(id, User);
        if (vm == null)
        {
            TempData["ErrorMessage"] = "Counselling session not found or you lack permission to view it.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Session Details - {vm.BeneficiaryCode}";
        return View(vm);
    }

    // GET: /Counselling/Create?beneficiaryId=12
    public async Task<IActionResult> Create(int? beneficiaryId)
    {
        ViewData["Title"] = "Record Counselling Session";
        var vm = await _counsellingService.GetCreateViewModelAsync(User, beneficiaryId);
        return View(vm);
    }

    // POST: /Counselling/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCounsellingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var fallback = await _counsellingService.GetCreateViewModelAsync(User, model.BeneficiaryId);
            model.BeneficiariesList = fallback.BeneficiariesList;
            model.TypesList = fallback.TypesList;
            model.StatusesList = fallback.StatusesList;
            ViewData["Title"] = "Record Counselling Session";
            return View(model);
        }

        var (success, sessionId, error) = await _counsellingService.CreateSessionAsync(model, User);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to save session.");
            var fallback = await _counsellingService.GetCreateViewModelAsync(User, model.BeneficiaryId);
            model.BeneficiariesList = fallback.BeneficiariesList;
            model.TypesList = fallback.TypesList;
            model.StatusesList = fallback.StatusesList;
            ViewData["Title"] = "Record Counselling Session";
            return View(model);
        }

        TempData["SuccessMessage"] = "Counselling session recorded successfully.";
        return RedirectToAction(nameof(Details), new { id = sessionId });
    }

    // GET: /Counselling/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _counsellingService.GetEditViewModelAsync(id, User);
        if (vm == null)
        {
            TempData["ErrorMessage"] = "Session not found or you lack permission to edit it.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Edit Session - {vm.BeneficiaryCode}";
        return View(vm);
    }

    // POST: /Counselling/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCounsellingViewModel model)
    {
        if (id != model.SessionId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            var fallback = await _counsellingService.GetEditViewModelAsync(id, User);
            if (fallback != null)
            {
                model.TypesList = fallback.TypesList;
                model.StatusesList = fallback.StatusesList;
            }
            ViewData["Title"] = $"Edit Session - {model.BeneficiaryCode}";
            return View(model);
        }

        var (success, error) = await _counsellingService.UpdateSessionAsync(model, User);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update session.");
            var fallback = await _counsellingService.GetEditViewModelAsync(id, User);
            if (fallback != null)
            {
                model.TypesList = fallback.TypesList;
                model.StatusesList = fallback.StatusesList;
            }
            ViewData["Title"] = $"Edit Session - {model.BeneficiaryCode}";
            return View(model);
        }

        TempData["SuccessMessage"] = "Counselling session updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.SessionId });
    }

    // POST: /Counselling/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _counsellingService.CancelSessionAsync(id, User);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to cancel session.";
        }
        else
        {
            TempData["SuccessMessage"] = "Counselling session has been marked as Cancelled.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
