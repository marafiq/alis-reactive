using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.CareLevelCascade;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/CareLevelCascade")]
public class CareLevelCascadeController : Controller
{
    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Conditions/CareLevelCascade/Index.cshtml", new CareLevelModel
        {
            CareLevel = "Assisted Living",
            Protocol = "Basic Support",
        });
}
