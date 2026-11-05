using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    public partial class ScheduleController
    {
        private const string SignOffsKeyPrefix = "shift-card-signoffs:";

        private static readonly IReadOnlyList<(int Id, string Subject, int StartHour, int EndHour)> TodaysShifts =
        [
            (1, "Morning medication pass", 9, 11),
            (2, "Afternoon activities", 13, 15),
        ];

        [HttpGet("ShiftCard")]
        public IActionResult ShiftCard()
            => View("~/Areas/Sandbox/Views/Components/Fusion/Schedule/ShiftCard.cshtml", new ShiftCardPageModel());

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
        public IActionResult ShiftCardDetails(int shiftId, int revision = 1)
        {
            var shift = TodaysShifts.FirstOrDefault(candidate => candidate.Id == shiftId);
            if (shift.Id == 0) return NotFound();

            return PartialView("~/Areas/Sandbox/Views/Components/Fusion/Schedule/_ShiftCardPartial.cshtml",
                new ShiftCardModel { ShiftId = shift.Id, Subject = shift.Subject, Revision = revision });
        }

        [HttpPost("ShiftCard/SignOff")]
        public IActionResult ShiftCardSignOff(int shiftId, int revision)
        {
            var shift = TodaysShifts.FirstOrDefault(candidate => candidate.Id == shiftId);
            if (shift.Id == 0) return NotFound();

            var key = SignOffsKeyPrefix + shiftId.ToString(CultureInfo.InvariantCulture);
            var signedOff = (HttpContext.Session.GetString(key) ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Append(revision.ToString(CultureInfo.InvariantCulture))
                .ToList();
            HttpContext.Session.SetString(key, string.Join(",", signedOff));

            var revisions = signedOff.Count == 1 ? $"revision {signedOff[0]}" : $"revisions {string.Join(" and ", signedOff)}";
            return Ok(new ShiftCardSignOffs($"{shift.Subject} handover signed off: {revisions}."));
        }
    }
}
