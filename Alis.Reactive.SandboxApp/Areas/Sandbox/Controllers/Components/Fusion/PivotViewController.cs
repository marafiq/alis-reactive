using System.Text.Json;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.PivotView;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The executive director's occupancy and care-hours report. A board snapshot keeps the report with its
    /// figures; the board packet takes the layout only and pulls the figures on the day of the meeting.
    /// Snapshots live in the browser's session, one per community, so each browser world keeps its own.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/PivotView")]
    public class PivotViewController : Controller
    {
        private const string DefaultCommunity = "maple-grove";

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index(string? community)
        {
            if (community == null)
                return RedirectToAction(nameof(Index), new { community = DefaultCommunity });
            if (!OccupancyCensusData.Communities.TryGetValue(community, out var name))
                return NotFound();

            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/PivotView/Index.cshtml",
                new PivotViewModel
                {
                    CommunityName = name,
                    Period = OccupancyCensusData.QuarterPeriod,
                    Census = OccupancyCensusData.Quarter
                });
        }

        [HttpGet("Census")]
        public IActionResult Census([FromQuery] OccupancyCensusQuery query) =>
            query.Community != null && OccupancyCensusData.Communities.ContainsKey(query.Community)
                ? Ok(new OccupancyCensus { Period = OccupancyCensusData.LatestPeriod, Rows = OccupancyCensusData.Latest })
                : NotFound();

        [HttpPost("Snapshots/{community}")]
        public IActionResult SaveSnapshot(string community, [FromBody] OccupancySnapshotLayout snapshot)
        {
            if (!OccupancyCensusData.Communities.TryGetValue(community, out var name))
                return NotFound();
            if (CensusMonths(snapshot.Layout).Count == 0)
                return BadRequest(new BoardReply { Message = "A snapshot keeps today's figures, and this layout carries none." });

            HttpContext.Session.SetString(SnapshotKey(community), snapshot.Layout);
            return Ok(new BoardReply { Message = $"Board snapshot saved for {name}." });
        }

        [HttpGet("Snapshots/{community}")]
        public IActionResult Snapshot(string community)
        {
            if (!OccupancyCensusData.Communities.TryGetValue(community, out var name))
                return NotFound();

            var layout = HttpContext.Session.GetString(SnapshotKey(community));
            if (layout == null)
                return NotFound(new BoardReply { Message = $"No board snapshot is saved for {name} yet." });

            var months = CensusMonths(layout);
            return Ok(new OccupancySnapshot
            {
                Message = $"Showing the board snapshot saved for {name}.",
                Layout = layout,
                Period = $"{months[0]} to {months[^1]}"
            });
        }

        [HttpPost("BoardPacket/{community}/{currentView}")]
        public IActionResult AddToBoardPacket(string community, string currentView, [FromBody] BoardPacketLayout packet)
        {
            var view = currentView switch
            {
                "Table" => "table",
                "Chart" => "chart",
                _ => null
            };
            if (view == null || !OccupancyCensusData.Communities.TryGetValue(community, out var name))
                return NotFound();
            if (CensusMonths(packet.Layout).Count > 0)
                return BadRequest(new BoardReply { Message = "The board packet takes the layout only; it pulls the figures on the day of the meeting." });

            return Ok(new BoardReply { Message = $"Added the {view} to {name}'s board packet." });
        }

        private static string SnapshotKey(string community) => "occupancySnapshot:" + community;

        // The months a layout's census rows cover, in the order the report shows them; none when the layout
        // was taken without its rows.
        private static IReadOnlyList<string> CensusMonths(string layout)
        {
            using var document = JsonDocument.Parse(layout);
            return document.RootElement
                .GetProperty("dataSourceSettings")
                .GetProperty("dataSource")
                .EnumerateArray()
                .Select(row => row.GetProperty("month").GetString() ?? "")
                .Distinct()
                .ToList();
        }
    }
}
