using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class FollowUpsController : Controller
{
    private readonly IFollowUpService _followUpService;
    private readonly ILogger<FollowUpsController> _logger;

    public FollowUpsController(
        IFollowUpService followUpService,
        ILogger<FollowUpsController> logger)
    {
        _followUpService = followUpService;
        _logger = logger;
    }

    // GET: /FollowUps
    public async Task<IActionResult> Index(
        string? search,
        string? filterTab,
        string? status,
        int page = 1)
    {
        ViewData["Title"] = "Follow-up Management";
        var vm = await _followUpService.GetFollowUpsAsync(
            User, search, filterTab, status, page, pageSize: 10);
        return View(vm);
    }

    // GET: /FollowUps/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _followUpService.GetFollowUpDetailsAsync(id, User);
        if (vm == null)
        {
            TempData["ErrorMessage"] = "Follow-up record not found or you lack permission to view it.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Follow-up Details - {vm.BeneficiaryCode}";
        return View(vm);
    }

    // GET: /FollowUps/Create?beneficiaryId=12
    public async Task<IActionResult> Create(int? beneficiaryId)
    {
        ViewData["Title"] = "Schedule Follow-up";
        var vm = await _followUpService.GetCreateViewModelAsync(User, beneficiaryId);
        return View(vm);
    }

    // POST: /FollowUps/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFollowUpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var fallback = await _followUpService.GetCreateViewModelAsync(User, model.BeneficiaryId);
            model.BeneficiariesList = fallback.BeneficiariesList;
            ViewData["Title"] = "Schedule Follow-up";
            return View(model);
        }

        var (success, followUpId, error) = await _followUpService.CreateFollowUpAsync(model, User);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to schedule follow-up.");
            var fallback = await _followUpService.GetCreateViewModelAsync(User, model.BeneficiaryId);
            model.BeneficiariesList = fallback.BeneficiariesList;
            ViewData["Title"] = "Schedule Follow-up";
            return View(model);
        }

        TempData["SuccessMessage"] = "Follow-up appointment scheduled successfully.";
        return RedirectToAction(nameof(Details), new { id = followUpId });
    }

    // GET: /FollowUps/Complete/5
    public async Task<IActionResult> Complete(int id)
    {
        var vm = await _followUpService.GetCompleteViewModelAsync(id, User);
        if (vm == null)
        {
            TempData["ErrorMessage"] = "Follow-up record not found or you lack permission to update it.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Record Outcome - {vm.BeneficiaryCode}";
        return View(vm);
    }

    // POST: /FollowUps/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, CompleteFollowUpViewModel model)
    {
        if (id != model.FollowUpId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Record Outcome - {model.BeneficiaryCode}";
            return View(model);
        }

        var (success, error) = await _followUpService.CompleteFollowUpAsync(model, User);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to save follow-up outcome.");
            ViewData["Title"] = $"Record Outcome - {model.BeneficiaryCode}";
            return View(model);
        }

        TempData["SuccessMessage"] = "Follow-up outcome recorded successfully.";
        return RedirectToAction(nameof(Details), new { id = model.FollowUpId });
    }

    // POST: /FollowUps/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _followUpService.CancelFollowUpAsync(id, User);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to cancel follow-up.";
        }
        else
        {
            TempData["SuccessMessage"] = "Follow-up appointment has been cancelled.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
