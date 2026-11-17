using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.Services;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class InterventionsController : Controller
{
    private readonly IInterventionService _interventionService;
    private readonly ILogger<InterventionsController> _logger;

    public InterventionsController(
        IInterventionService interventionService,
        ILogger<InterventionsController> logger)
    {
        _interventionService = interventionService;
        _logger = logger;
    }

    // GET: /Interventions
    public async Task<IActionResult> Index(
        string? search,
        int? beneficiaryId,
        string? interventionType,
        string? status,
        int page = 1)
    {
        ViewData["Title"] = "De-Addiction Interventions";
        var vm = await _interventionService.GetInterventionsAsync(
            User, search, beneficiaryId, interventionType, status, page, pageSize: 10);
        return View(vm);
    }

    // GET: /Interventions/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _interventionService.GetInterventionDetailsAsync(id, User);
        if (vm == null)
        {
            TempData["ErrorMessage"] = "Intervention record not found or you lack permission to view it.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Intervention Details - {vm.BeneficiaryCode}";
        return View(vm);
    }

    // GET: /Interventions/Create?beneficiaryId=12
    public async Task<IActionResult> Create(int? beneficiaryId)
    {
        ViewData["Title"] = "Record Treatment Intervention";
        var vm = await _interventionService.GetCreateViewModelAsync(User, beneficiaryId);
        return View(vm);
    }

    // POST: /Interventions/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInterventionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var fallback = await _interventionService.GetCreateViewModelAsync(User, model.BeneficiaryId);
            model.BeneficiariesList = fallback.BeneficiariesList;
            model.TypesList = fallback.TypesList;
            model.StatusesList = fallback.StatusesList;
            ViewData["Title"] = "Record Treatment Intervention";
            return View(model);
        }

        var (success, interventionId, error) = await _interventionService.CreateInterventionAsync(model, User);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to save intervention.");
            var fallback = await _interventionService.GetCreateViewModelAsync(User, model.BeneficiaryId);
            model.BeneficiariesList = fallback.BeneficiariesList;
            model.TypesList = fallback.TypesList;
            model.StatusesList = fallback.StatusesList;
            ViewData["Title"] = "Record Treatment Intervention";
            return View(model);
        }

        TempData["SuccessMessage"] = "Intervention record created successfully.";
        return RedirectToAction(nameof(Details), new { id = interventionId });
    }

    // GET: /Interventions/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _interventionService.GetEditViewModelAsync(id, User);
        if (vm == null)
        {
            TempData["ErrorMessage"] = "Intervention record not found or you lack permission to edit it.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Edit Intervention - {vm.BeneficiaryCode}";
        return View(vm);
    }

    // POST: /Interventions/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditInterventionViewModel model)
    {
        if (id != model.InterventionId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            var fallback = await _interventionService.GetEditViewModelAsync(id, User);
            if (fallback != null)
            {
                model.TypesList = fallback.TypesList;
                model.StatusesList = fallback.StatusesList;
            }
            ViewData["Title"] = $"Edit Intervention - {model.BeneficiaryCode}";
            return View(model);
        }

        var (success, error) = await _interventionService.UpdateInterventionAsync(model, User);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update intervention.");
            var fallback = await _interventionService.GetEditViewModelAsync(id, User);
            if (fallback != null)
            {
                model.TypesList = fallback.TypesList;
                model.StatusesList = fallback.StatusesList;
            }
            ViewData["Title"] = $"Edit Intervention - {model.BeneficiaryCode}";
            return View(model);
        }

        TempData["SuccessMessage"] = "Intervention record updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.InterventionId });
    }

    // POST: /Interventions/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _interventionService.CancelInterventionAsync(id, User);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to update intervention status.";
        }
        else
        {
            TempData["SuccessMessage"] = "Intervention marked as Discontinued.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
