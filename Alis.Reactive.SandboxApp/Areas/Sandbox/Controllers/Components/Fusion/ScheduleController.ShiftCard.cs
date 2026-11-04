using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Shift cards: today's floor schedule, where each shift's card loads its handover from the server.
    /// Schedule rebuilds the card each time it opens, so the charge nurse opens and closes cards all shift.
    /// </summary>
    public partial class ScheduleController
    {
        private static readonly IReadOnlyList<(int Id, string Subject, int StartHour, int EndHour)> TodaysShifts =
        [
            (1, "Morning medication pass", 9, 11),
            (2, "Afternoon activities", 13, 15),
        ];

        [HttpGet("ShiftCard")]
        public IActionResult ShiftCard() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/Schedule/ShiftCard.cshtml", new ShiftCardPageModel());

        [HttpGet("ShiftCard/Shifts")]
        public IActionResult ShiftCardShifts()
        {
            var today = DateTime.Today;
            var shifts = TodaysShifts
                .Select(shift => new ShiftCardShift(
                    shift.Id, shift.Subject, today.AddHours(shift.StartHour), today.AddHours(shift.EndHour)))
                .ToList();
            return Ok(new ShiftCardDay(shifts, $"{shifts.Count} shifts on the floor today."));
        }

        [HttpGet("ShiftCard/Details")]
        public IActionResult ShiftCardDetails(int shiftId)
        {
            var shift = TodaysShifts.Single(candidate => candidate.Id == shiftId);
            return PartialView("~/Areas/Sandbox/Views/Components/Fusion/Schedule/_ShiftCardPartial.cshtml",
                new ShiftCardModel { ShiftId = shift.Id, Subject = shift.Subject });
        }
    }
}
