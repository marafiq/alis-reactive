using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.EscalationRouting;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/EscalationRouting")]
public class EscalationRoutingController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Areas/Sandbox/Views/Conditions/EscalationRouting/Index.cshtml", new EscalationRoutingModel());
    }
}
