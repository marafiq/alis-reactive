using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/HoldRelease")]
    public class HoldReleaseController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Brought/HoldRelease/Index.cshtml", new HoldReleaseModel());
    }
}
