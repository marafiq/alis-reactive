using System.Collections.Concurrent;
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
                Summary = $"{currentView}:{request.FacilityId}",
                LayoutLength = request.Layout.Length
            });
        }

        // Saved views, one per facility; each test opens the page for a facility of its own.
        private static readonly ConcurrentDictionary<string, string> SavedViews = new();

        [HttpPost("~/api/pivot/views/{facilityId}")]
        public IActionResult SaveView(string facilityId, [FromBody] PivotSavedView view)
        {
            SavedViews[facilityId] = view.Layout;
            return Ok(new PivotLayoutResponse { Message = "View saved.", Layout = view.Layout });
        }

        [HttpGet("~/api/pivot/views/{facilityId}")]
        public IActionResult SavedView(string facilityId)
        {
            return SavedViews.TryGetValue(facilityId, out var layout)
                ? Ok(new PivotLayoutResponse { Message = "Saved view restored.", Layout = layout })
                : NotFound(new PivotLayoutResponse { Message = "No view is saved for this facility." });
        }
    }
}
