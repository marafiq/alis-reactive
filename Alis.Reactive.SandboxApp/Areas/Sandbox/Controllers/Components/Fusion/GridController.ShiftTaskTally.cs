using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Shift task tally: at shift change the evening charge nurse updates each resident's open tasks and
    /// PRN doses, reviews the changes and sends them to the night shift. Every page starts from the
    /// evening's counts; a review or a send answers with what the changes come to and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("ShiftTaskTally")]
        public IActionResult ShiftTaskTally() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/ShiftTaskTally.cshtml",
                new ShiftTaskTallyModel { Residents = ShiftTallyOnTheWing() });

        // The eMAR's evening medication pass left Grace Bennett with tasks the nurse's count does not have.
        [HttpGet("ShiftTaskTallyEmar")]
        public IActionResult ShiftTaskTallyEmar() =>
            Ok(new EmarTaskCount
            {
                ResidentId = 6202,
                OpenTasks = 5,
                Summary = "eMAR: Grace Bennett's evening med pass leaves 5 open tasks."
            });

        [HttpPost("ShiftTaskTallyReview")]
        public IActionResult ShiftTaskTallyReview([FromBody] ShiftTallyChanges? changes)
        {
            var changed = changes?.BatchChanges?.ChangedRecords ?? new List<ShiftTallyResident>();
            return Ok(new ShiftTallySummary
            {
                Summary = changed.Count == 0
                    ? "No changes in the tally yet."
                    : $"Changed for the night shift: {DescribeTally(changed)}."
            });
        }

        [HttpPost("ShiftTaskTallySend")]
        public IActionResult ShiftTaskTallySend([FromBody] ShiftTallyChanges? changes)
        {
            var changed = changes?.BatchChanges?.ChangedRecords ?? new List<ShiftTallyResident>();
            return Ok(new ShiftTallySummary
            {
                Summary = changed.Count == 0
                    ? "Nothing changed, so nothing was sent."
                    : $"Sent to the night shift: {DescribeTally(changed)}."
            });
        }

        private static string DescribeTally(IEnumerable<ShiftTallyResident> changed) =>
            string.Join("; ", changed.Select(r =>
                $"{r.ResidentName}, {Counted(r.OpenTasks, "open task")} and {Counted(r.PrnDoses, "PRN dose")}"));

        private static string Counted(int count, string thing) => count == 1 ? $"1 {thing}" : $"{count} {thing}s";

        private static List<ShiftTallyResident> ShiftTallyOnTheWing() => new()
        {
            new ShiftTallyResident { ResidentId = 6201, ResidentName = "Amina Patel", Suite = "101", OpenTasks = 2, PrnDoses = 0 },
            new ShiftTallyResident { ResidentId = 6202, ResidentName = "Grace Bennett", Suite = "104", OpenTasks = 3, PrnDoses = 1 },
            new ShiftTallyResident { ResidentId = 6203, ResidentName = "Henry Liu", Suite = "108", OpenTasks = 6, PrnDoses = 0 },
            new ShiftTallyResident { ResidentId = 6204, ResidentName = "Irene Morgan", Suite = "112", OpenTasks = 4, PrnDoses = 2 }
        };
    }
}
