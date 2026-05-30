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
    }
}
