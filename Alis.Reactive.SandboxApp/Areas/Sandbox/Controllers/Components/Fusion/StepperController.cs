using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionStepper")]
    public class StepperController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Stepper/Index.cshtml",
                new FusionStepperModel());
        }
    }
}
