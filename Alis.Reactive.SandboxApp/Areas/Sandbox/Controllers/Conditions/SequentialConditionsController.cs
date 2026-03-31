using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/SequentialConditions")]
public class SequentialConditionsController : Controller
{
    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Conditions/SequentialConditions/Index.cshtml");
}
