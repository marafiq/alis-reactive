using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ContextMenu;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/ContextMenu")]
    public class ContextMenuController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/ContextMenu/Index.cshtml",
                new FusionContextMenuModel());
        }
    }
}
