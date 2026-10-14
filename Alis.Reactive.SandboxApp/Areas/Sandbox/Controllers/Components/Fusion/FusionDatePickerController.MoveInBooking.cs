using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>Move-in booking: the server confirms the days it booked, written out in full.</summary>
    public partial class FusionDatePickerController
    {
        [HttpGet("MoveInBooking")]
        public IActionResult MoveInBooking() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/FusionDatePicker/MoveInBooking.cshtml", new MoveInBookingModel());

        [HttpPost("MoveInBooking/Book")]
        public IActionResult BookMoveIn([FromBody] MoveInBookingModel? booking)
        {
            if (booking?.MoveInDay == null) return BadRequest();

            var confirmation = $"Move-in booked for {Spelled(booking.MoveInDay.Value.ToDateTime(TimeOnly.MinValue))}.";
            if (booking.WelcomeVisit.HasValue)
                confirmation += $" Welcome visit on {Spelled(booking.WelcomeVisit.Value)}.";
            if (booking.TruckArrives.HasValue)
                confirmation += $" The truck arrives at {booking.TruckArrives.Value.ToUniversalTime().ToString("HH:mm", CultureInfo.InvariantCulture)} UTC.";
            return Ok(new MoveInBooked(confirmation));
        }

        private static string Spelled(DateTime day) => day.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
    }
}
