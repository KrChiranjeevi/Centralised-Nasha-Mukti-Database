using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>Beneficiaries CRUD — Phase 5 will add full DB operations.</summary>
public class BeneficiariesController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Beneficiaries"; return View(); }
    public IActionResult Details(int id) { ViewData["Title"] = "Beneficiary Details"; return View(); }
}
