using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class BeneficiariesController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Beneficiaries"; return View(); }
    public IActionResult Details(int id) { ViewData["Title"] = "Beneficiary Details"; return View(); }
}
