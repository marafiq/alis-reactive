using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.BulletChart;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionBulletChart")]
    public class BulletChartController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/BulletChart/Index.cshtml",
                new FusionBulletChartModel());
        }
    }
}
