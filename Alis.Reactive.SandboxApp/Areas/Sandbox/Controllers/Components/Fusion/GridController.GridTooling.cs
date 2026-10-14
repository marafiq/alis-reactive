using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Next week's shift schedule: the staffing scheduler reads the week a day at a time, jumps to the weekend
    /// and chooses the columns to show. Every page starts from the schedule on file.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("GridTooling"), ActionName("GridTooling")]
        public IActionResult ShiftSchedulePage() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/Grid/GridTooling.cshtml", new ShiftScheduleViewModel());

        // Monday first, then each day's day, evening and night shift in turn.
        [HttpGet("GridToolingShifts")]
        public IActionResult GridToolingShifts()
        {
            var staffed = new (string Day, string Nurse, string Aide)[]
            {
                ("Mon, Oct 19", "Nora Ellis", "Sam Ortiz"), ("Mon, Oct 19", "Malik Stone", "Priya Shah"), ("Mon, Oct 19", "Elena Ruiz", "Leo Simmons"),
                ("Tue, Oct 20", "Nora Ellis", "Sam Ortiz"), ("Tue, Oct 20", "Malik Stone", "Priya Shah"), ("Tue, Oct 20", "Owen Park", "Leo Simmons"),
                ("Wed, Oct 21", "Elena Ruiz", "Sam Ortiz"), ("Wed, Oct 21", "Malik Stone", "Ada Brooks"), ("Wed, Oct 21", "Owen Park", "Leo Simmons"),
                ("Thu, Oct 22", "Nora Ellis", "Ada Brooks"), ("Thu, Oct 22", "Elena Ruiz", "Priya Shah"), ("Thu, Oct 22", "Owen Park", "Leo Simmons"),
                ("Fri, Oct 23", "Nora Ellis", "Sam Ortiz"), ("Fri, Oct 23", "Malik Stone", "Priya Shah"), ("Fri, Oct 23", "Elena Ruiz", "Ada Brooks"),
                ("Sat, Oct 24", "Grace Okafor", "Tom Reyes"), ("Sat, Oct 24", "Malik Stone", "Open"), ("Sat, Oct 24", "Open", "Leo Simmons"),
                ("Sun, Oct 25", "Grace Okafor", "Tom Reyes"), ("Sun, Oct 25", "Owen Park", "Ada Brooks"), ("Sun, Oct 25", "Open", "Open")
            };
            var shifts = new[] { "Day", "Evening", "Night" };
            return Ok(staffed.Select((slot, index) => new ScheduledShift
            {
                ShiftId = 8001 + index,
                Day = slot.Day,
                Shift = shifts[index % shifts.Length],
                Nurse = slot.Nurse,
                Aide = slot.Aide
            }).ToList());
        }
    }
}
