using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Cardiology follow-up: the clinic says whether Dr. Osei is in at the time picked, and books the time
    /// the nurse settles on, written out in full.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/DateTimePicker")]
    public class DateTimePickerController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/DateTimePicker/Index.cshtml", new DateTimePickerModel());
        }

        [HttpGet("ClinicHours")]
        public IActionResult ClinicHours([FromQuery] ClinicHoursQuery query)
        {
            if (query.Visit is not { } visit) return BadRequest();

            var inClinic = DateTimePickerModel.InClinicHours(visit);
            var answer = inClinic
                ? $"Dr. Osei is in clinic on {WrittenOut(visit)}."
                : $"Dr. Osei is not in clinic on {WrittenOut(visit)}.";
            return Ok(new ClinicHoursAnswer(visit, inClinic, answer));
        }

        [HttpPost("ScheduleFollowUp")]
        public IActionResult ScheduleFollowUp([FromBody] DateTimePickerModel? followUp)
        {
            if (followUp == null) return BadRequest();

            var result = new FollowUpAppointmentValidator().Validate(followUp);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new FollowUpBooked($"Arthur Bell sees Dr. Osei on {WrittenOut(followUp.FollowUpAppointment!.Value)}."));
        }

        private static string WrittenOut(DateTime visit) =>
            visit.ToString("dddd, MMMM d 'at' h:mm tt", CultureInfo.InvariantCulture);
    }
}
