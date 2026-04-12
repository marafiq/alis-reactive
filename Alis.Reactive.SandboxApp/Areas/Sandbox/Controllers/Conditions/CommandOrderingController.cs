using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/CommandOrdering")]
public class CommandOrderingController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Areas/Sandbox/Views/Conditions/CommandOrdering/Index.cshtml",
            new Models.Conditions.CommandOrdering.CommandOrderingModel());
    }
}
