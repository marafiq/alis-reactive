using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.RoomWhiteboard;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.CoreBehaviors
{
    /// <summary>
    /// Room 12's whiteboard: every kind of reaction, each writing something the nurse sees.
    /// URL: /Sandbox/CoreBehaviors/RoomWhiteboard.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/CoreBehaviors/RoomWhiteboard")]
    public class RoomWhiteboardController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/CoreBehaviors/RoomWhiteboard/Index.cshtml", new RoomWhiteboardModel());
    }
}
