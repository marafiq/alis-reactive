using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/PivotView")]
    public class PivotViewController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/PivotView/Index.cshtml",
                new PivotViewModel());
        }

        [HttpGet("~/api/pivot/census")]
        public IActionResult Census(string? facilityId)
        {
            var label = string.IsNullOrWhiteSpace(facilityId) ? "facility" : facilityId;
            return Ok(new PivotCensusResponse
            {
                Message = $"March census loaded for {label}",
                Rows = PivotCensusData.MarchRows
            });
        }

        [HttpPost("~/api/pivot/{currentView}/audit")]
        public IActionResult Audit(string currentView, [FromBody] PivotAuditRequest request)
        {
            return Ok(new PivotAuditResponse
            {
                Summary = $"{currentView}:{request.FacilityId}:{request.CurrentView}",
                LayoutLength = request.Layout.Length
            });
        }

        [HttpPost("~/api/pivot/layout/echo")]
        public IActionResult EchoLayout([FromBody] PivotLayoutRequest request)
        {
            return Ok(new PivotLayoutResponse
            {
                Message = $"layout echoed:{request.Layout.Length}",
                Layout = request.Layout
            });
        }
    }
}
