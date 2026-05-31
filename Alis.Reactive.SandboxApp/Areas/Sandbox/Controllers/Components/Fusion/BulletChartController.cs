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

        [HttpPost("ClickAudit")]
        public IActionResult ClickAudit([FromBody] FusionBulletChartClickAuditRequest request)
        {
            var wing = request.Target.Contains("FeatureMeasure_0", StringComparison.Ordinal)
                ? "North Wing"
                : "Unknown wing";

            return Ok(new FusionBulletChartClickAuditResponse
            {
                Wing = wing,
                Coordinates = $"{Math.Round(request.X)},{Math.Round(request.Y)}",
                Message = $"Opened readiness drilldown for {wing}"
            });
        }
    }
}
