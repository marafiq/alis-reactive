using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    public partial class GridController
    {
        [HttpGet("BatchRosterChange"), ActionName("BatchRosterChange")]
        public IActionResult SuiteTurnoverPage()
            => View(
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
