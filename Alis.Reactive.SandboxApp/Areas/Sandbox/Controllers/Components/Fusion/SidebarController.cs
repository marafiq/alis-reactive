using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Sidebar;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionSidebar")]
    public class SidebarController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Sidebar/Index.cshtml",
                new FusionSidebarModel());
        }

        [HttpPost("OpenPanel")]
        public IActionResult OpenPanel([FromBody] FusionSidebarOpenRequest request)
        {
            var openedNote = request.OpenedByCoordinator
                ? "You opened the care-services menu."
                : "Care-services menu opened.";

            return Ok(new FusionSidebarOpenResponse
            {
                ServicesSummary = "3 care services available",
                OpenedNote = openedNote
            });
        }

        [HttpPost("CloseActivity")]
        public IActionResult CloseActivity([FromBody] FusionSidebarCloseRequest request)
        {
            var activityNote = request.IsOpen
                ? "Care-services menu is still open."
                : "Care-services menu closed — services hidden.";

            return Ok(new FusionSidebarCloseResponse
            {
                ActivityNote = activityNote
            });
        }
    }
}
