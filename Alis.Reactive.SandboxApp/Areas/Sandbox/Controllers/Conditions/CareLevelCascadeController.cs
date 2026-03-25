using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/CareLevelCascade")]
public class CareLevelCascadeController : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        ViewBag.CareLevels = new List<string>
        {
            "Independent", "Assisted Living", "Memory Care", "Skilled Nursing"
        };
        ViewBag.Protocols = new List<string>
        {
            "", "Basic Support", "Enhanced Monitoring", "Full Clinical"
        };
        return View("~/Areas/Sandbox/Views/Conditions/CareLevelCascade/Index.cshtml", new CareLevelModel());
    }
}
