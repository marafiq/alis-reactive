using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.NightRound;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.CoreBehaviors
{
    [Area("Sandbox")]
    [Route("Sandbox/CoreBehaviors/NightRound")]
    public class NightRoundController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/CoreBehaviors/NightRound/Index.cshtml", new NightRoundModel());
    }
}
