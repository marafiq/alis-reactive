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

        // A wing's bar and its budget line end with the wing's place in the chart, as EJ2 names them.
        [HttpPost("WingShifts")]
        public IActionResult WingShifts([FromBody] WingShiftsRequest request)
        {
            var place = request.Bar[(request.Bar.LastIndexOf('_') + 1)..];
            if (!int.TryParse(place, out var index) || index < 0 || index >= StaffingWeek.Nursing.Count)
                return NotFound();

            var wing = StaffingWeek.Nursing[index].Wing;
            return Ok(new WingShifts { Wing = wing, Summary = StaffingWeek.ShiftNotes[wing] });
        }
    }
}
