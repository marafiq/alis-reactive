using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Menu;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionMenu")]
    public class MenuController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Menu/Index.cshtml",
                new FusionMenuModel());
        }
    }
}
