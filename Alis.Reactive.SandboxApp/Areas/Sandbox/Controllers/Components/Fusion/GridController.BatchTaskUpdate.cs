using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The day task board: the Birch wing day nurse keeps each resident's open tasks current and hands the board to the
    /// evening shift. Every page starts from the counts on record; a handover answers with the board as saved and what
    /// changed against the record, and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("BatchTaskUpdate"), ActionName("BatchTaskUpdate")]
        public IActionResult DayTaskBoardPage() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/Grid/BatchTaskUpdate.cshtml", new DayTaskBoardViewModel());

        [HttpGet("DayTaskBoardResidents")]
        public IActionResult DayTaskBoardResidents() => Ok(DayTaskBoardOnRecord());

        [HttpPost("DayTaskBoardHandover")]
        public IActionResult DayTaskBoardHandover([FromBody] DayTaskBoardChanges handover)
        {
            var board = DayTaskBoardOnRecord();
            var changes = new List<string>();
            foreach (var changed in handover.Changes.ChangedRecords)
            {
                var resident = board.Single(r => r.ResidentId == changed.ResidentId);
                if (resident.OpenTasks != changed.OpenTasks)
                    changes.Add($"{resident.ResidentName}, {resident.OpenTasks} → {changed.OpenTasks} open tasks");
                resident.OpenTasks = changed.OpenTasks;
            }

            return Ok(new DayTaskBoardHandedOver
            {
                Residents = board,
                Summary = changes.Count == 0
                    ? "No open tasks changed, so the evening shift starts from the counts on record."
                    : $"Handed over to the evening shift: {string.Join("; ", changes)}."
            });
        }

        private static List<DayTaskBoardResident> DayTaskBoardOnRecord() => new()
        {
            new DayTaskBoardResident { ResidentId = 6301, ResidentName = "Amina Patel", Room = "101", OpenTasks = 2 },
            new DayTaskBoardResident { ResidentId = 6302, ResidentName = "Henry Liu", Room = "104", OpenTasks = 3 },
            new DayTaskBoardResident { ResidentId = 6303, ResidentName = "Grace Bennett", Room = "108", OpenTasks = 1 },
            new DayTaskBoardResident { ResidentId = 6304, ResidentName = "Walter Grant", Room = "112", OpenTasks = 0 },
            new DayTaskBoardResident { ResidentId = 6305, ResidentName = "Edith Hale", Room = "115", OpenTasks = 4 },
            new DayTaskBoardResident { ResidentId = 6306, ResidentName = "Jonah Reed", Room = "118", OpenTasks = 2 }
        };
    }
}
