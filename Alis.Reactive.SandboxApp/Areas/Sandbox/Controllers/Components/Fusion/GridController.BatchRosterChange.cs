using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Suite 205's turnover on Maple wing: the admissions coordinator records Ruth Carter's move-out and Walter Kim's
    /// move-in as one change to the roster and saves it once housekeeping has turned the suite over. Every page starts
    /// from the roster on record; a save answers with what the turnover recorded and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("BatchRosterChange"), ActionName("BatchRosterChange")]
        public IActionResult SuiteTurnoverPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/BatchRosterChange.cshtml",
                new SuiteTurnoverViewModel
                {
                    Departure = SuiteTurnoverOnRecord().Single(r => r.ResidentId == 6604),
                    Arrival = new SuiteTurnoverResident { ResidentId = 6610, ResidentName = "Walter Kim", Suite = "205", CareLevel = "Assisted Living" }
                });

        [HttpGet("SuiteTurnoverRoster")]
        public IActionResult SuiteTurnoverRoster() => Ok(SuiteTurnoverOnRecord());

        [HttpPost("SuiteTurnoverSave")]
        public IActionResult SuiteTurnoverSave([FromBody] SuiteTurnoverChanges turnover)
        {
            var moves = turnover.Changes.DeletedRecords.Select(r => $"{r.ResidentName} moved out of suite {r.Suite}")
                .Concat(turnover.Changes.AddedRecords.Select(r => $"{r.ResidentName} moved into suite {r.Suite}"))
                .ToList();

            return Ok(new SuiteTurnoverRecorded
            {
                Summary = moves.Count == 0
                    ? "No move was saved."
                    : $"Turnover saved: {string.Join(" and ", moves)}."
            });
        }

        private static List<SuiteTurnoverResident> SuiteTurnoverOnRecord() => new()
        {
            new SuiteTurnoverResident { ResidentId = 6601, ResidentName = "Amina Patel", Suite = "201", CareLevel = "Assisted Living" },
            new SuiteTurnoverResident { ResidentId = 6602, ResidentName = "Henry Liu", Suite = "203", CareLevel = "Assisted Living" },
            new SuiteTurnoverResident { ResidentId = 6603, ResidentName = "Grace Bennett", Suite = "204", CareLevel = "Independent" },
            new SuiteTurnoverResident { ResidentId = 6604, ResidentName = "Ruth Carter", Suite = "205", CareLevel = "Assisted Living" },
            new SuiteTurnoverResident { ResidentId = 6605, ResidentName = "Jonah Reed", Suite = "207", CareLevel = "Independent" }
        };
    }
}
