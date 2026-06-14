using System;
using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/TimePicker")]
    public class TimePickerController : Controller
    {
        // The medication time currently on the resident's record, carried over into
        // the scheduler so the coordinator reviews it before changing it. The date
        // portion is a real recent day so the Syncfusion server-side render uses the
        // modern whole-hour timezone offset; the TimePicker uses only the time-of-day.
        private static readonly DateTime CurrentMedicationTime =
            new DateTime(DateTime.Today.Year, 1, 1, 9, 0, 0);

        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/TimePicker/Index.cshtml",
                new TimePickerModel { MedicationTime = CurrentMedicationTime });
        }

        [HttpPost("Schedule")]
        public IActionResult Schedule([FromBody] MedicationScheduleRequest request)
        {
            var when = request.MedicationTime?.ToString("HH:mm", CultureInfo.InvariantCulture)
                ?? "an unscheduled time";

            return Ok(new MedicationScheduleResponse
            {
                MedicationTime = request.MedicationTime,
                Confirmation = "Morning medication scheduled for " + when + "."
            });
        }
    }
}
