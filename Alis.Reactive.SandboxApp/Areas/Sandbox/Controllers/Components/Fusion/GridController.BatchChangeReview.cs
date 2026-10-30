using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Night safety checks on Cedar wing: the charge nurse corrects the two-hourly checks the aides recorded, reviews the
    /// changes and saves them to the night log. Every page starts from the checks the aides recorded; a save answers with
    /// the checks as the log now holds them and what was saved, and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("BatchChangeReview"), ActionName("BatchChangeReview")]
        public IActionResult NightChecksPage() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/Grid/BatchChangeReview.cshtml", new NightChecksViewModel());

        [HttpGet("NightChecksTonight")]
        public IActionResult NightChecksTonight() => Ok(NightChecksRecorded());

        [HttpPost("NightChecksLog")]
        public IActionResult NightChecksLog([FromBody] NightCheckChanges review)
        {
            var log = NightChecksRecorded();
            var saved = new List<string>();
            foreach (var changed in review.Changes.ChangedRecords)
            {
                var resident = log.Single(r => r.ResidentId == changed.ResidentId);
                if (resident.ChecksDone != changed.ChecksDone)
                    saved.Add($"{resident.ResidentName}, {changed.ChecksDone} {(changed.ChecksDone == 1 ? "check" : "checks")}");
                resident.ChecksDone = changed.ChecksDone;
            }

            return Ok(new NightChecksLogged
            {
                Residents = log,
                Summary = saved.Count == 0
                    ? "No checks changed, so nothing was added to the night log."
                    : $"Saved to the night log: {string.Join("; ", saved)}."
            });
        }

        private static List<NightCheckResident> NightChecksRecorded() => new()
        {
            new NightCheckResident { ResidentId = 6501, ResidentName = "Amina Patel", Room = "301", ChecksDone = 3 },
            new NightCheckResident { ResidentId = 6502, ResidentName = "Henry Liu", Room = "304", ChecksDone = 2 },
            new NightCheckResident { ResidentId = 6503, ResidentName = "Grace Bennett", Room = "308", ChecksDone = 3 },
            new NightCheckResident { ResidentId = 6504, ResidentName = "Walter Grant", Room = "312", ChecksDone = 1 },
            new NightCheckResident { ResidentId = 6505, ResidentName = "Edith Hale", Room = "315", ChecksDone = 3 }
        };
    }
}
