using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.NightRound;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.CoreBehaviors
{
    /// <summary>
    /// The night round: every kind of trigger, each starting something the nurse sees.
    /// URL: /Sandbox/CoreBehaviors/NightRound.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/CoreBehaviors/NightRound")]
    public class NightRoundController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/CoreBehaviors/NightRound/Index.cshtml", new NightRoundModel());
    }
}
