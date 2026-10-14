using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The evening task count: the charge nurse records each resident's open tasks and missed doses on the
    /// Maple wing and saves the count at handoff. Every page starts from the counts on file; the server
    /// describes a count against them and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("BatchCellEdit"), ActionName("BatchCellEdit")]
        public IActionResult TaskCountPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/BatchCellEdit.cshtml",
                new TaskCountViewModel { Residents = TaskCountOnFile() });

        [HttpPost("TaskCountReview")]
        public IActionResult TaskCountReview([FromBody] TaskCountChanges request)
        {
            var changes = TaskCountChangeWords(request.Changes.ChangedRecords);
            return Ok(new TaskCountSummary
            {
                Summary = changes.Count == 0 ? "No counts changed yet." : $"Changed so far: {string.Join("; ", changes)}."
            });
        }

        [HttpPost("TaskCountSave")]
        public IActionResult TaskCountSave([FromBody] TaskCountChanges request)
        {
            var changes = TaskCountChangeWords(request.Changes.ChangedRecords);
            return Ok(new TaskCountSummary
            {
                Summary = changes.Count == 0 ? "Nothing to save: no count changed." : $"Count saved: {string.Join("; ", changes)}."
            });
        }

        // The task board has one count the sheet does not: Henry Liu's open tasks went up during the shift.
        [HttpPost("TaskCountTaskBoard")]
        public IActionResult TaskCountTaskBoard()
        {
            var sheet = TaskCountOnFile();
            var row = sheet.FindIndex(resident => resident.Room == 202);
            return Ok(new TaskBoardCount
            {
                RowIndex = row,
                OpenTasks = 6,
                Summary = $"The task board shows 6 open tasks for {sheet[row].ResidentName}."
            });
        }

        // "Amina Patel, open tasks 2 → 4" for each resident whose counts differ from the counts on file.
        private static List<string> TaskCountChangeWords(IEnumerable<TaskCountRow> changed)
        {
            var onFile = TaskCountOnFile().ToDictionary(resident => resident.Room);
            var words = new List<string>();
            foreach (var row in changed)
            {
                var was = onFile[row.Room];
                var counts = new List<string>();
                if (was.OpenTasks != row.OpenTasks)
                    counts.Add($"open tasks {was.OpenTasks} → {row.OpenTasks}");
                if (was.MissedDoses != row.MissedDoses)
                    counts.Add($"missed doses {was.MissedDoses} → {row.MissedDoses}");
                if (counts.Count > 0)
                    words.Add($"{row.ResidentName}, {string.Join(", ", counts)}");
            }

            return words;
        }

        private static List<TaskCountRow> TaskCountOnFile() => new()
        {
            new TaskCountRow { Room = 201, ResidentName = "Amina Patel", OpenTasks = 2, MissedDoses = 0 },
            new TaskCountRow { Room = 202, ResidentName = "Henry Liu", OpenTasks = 3, MissedDoses = 1 },
            new TaskCountRow { Room = 203, ResidentName = "Grace Bennett", OpenTasks = 1, MissedDoses = 0 },
            new TaskCountRow { Room = 204, ResidentName = "Walter Grant", OpenTasks = 0, MissedDoses = 0 },
            new TaskCountRow { Room = 205, ResidentName = "Edith Hale", OpenTasks = 4, MissedDoses = 2 },
            new TaskCountRow { Room = 206, ResidentName = "Jonah Reed", OpenTasks = 2, MissedDoses = 0 },
            new TaskCountRow { Room = 207, ResidentName = "Priya Shah", OpenTasks = 1, MissedDoses = 0 },
            new TaskCountRow { Room = 208, ResidentName = "Ruth Carter", OpenTasks = 5, MissedDoses = 1 }
        };
    }
}
