using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/CallBell")]
    public class CallBellController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Brought/CallBell/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new NightCallModel());

        // The desk's card while someone is on duty: its plan listens to the room's bell.
        [HttpGet("DeskOnDuty")]
        public IActionResult DeskOnDuty() => PartialView(ViewBase + "_DeskOnDuty.cshtml", new DeskOnDutyModel());

        [HttpGet("NoDesk")]
        public IActionResult NoDesk() => PartialView(ViewBase + "_NoDesk.cshtml");
    }
}
