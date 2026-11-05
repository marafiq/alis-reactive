using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// Training sessions for the week of January 11, 2027: Dementia care on Monday at 9:00 AM and Fall
    /// prevention on Wednesday at 2:00 PM. Each answer reads a time as the clock the coordinator picked.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/TrainingSessions")]
    public class TrainingSessionsController : Controller
    {
        private static readonly DateTime WeekOf = new(2027, 1, 11);

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/HttpPipeline/TrainingSessions/Index.cshtml", new TrainingSessionModel());

        [HttpGet("Week")]
        public IActionResult Week() => Ok(new TrainingWeek(
        [
            new(1, "Dementia care", WeekOf.AddHours(9), WeekOf.AddHours(10).AddMinutes(30)),
            new(2, "Fall prevention", WeekOf.AddDays(2).AddHours(14), WeekOf.AddDays(2).AddHours(15)),
        ]));

        [HttpGet("Room")]
        public IActionResult Room([FromQuery] DateTimeOffset? startsAt)
        {
            if (startsAt == null) return BadRequest();

            return Ok(new TrainingRoomAnswer($"Room 2 is free at {Clock(startsAt.Value)}."));
        }

        [HttpPost("Book")]
        public IActionResult Book([FromBody] TrainingSessionModel? session)
        {
            if (session == null) return BadRequest();

            var result = new TrainingSessionValidator().Validate(session);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            var startsAt = session.StartsAt!.Value;
            var day = startsAt.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
            var offset = startsAt.ToString("zzz", CultureInfo.InvariantCulture);
            return Ok(new TrainingSessionBooked($"Booked for {day} at {Clock(startsAt)} (UTC{offset})."));
        }

        [HttpPost("ConfirmWeek")]
        public IActionResult ConfirmWeek([FromBody] TrainingWeekConfirmation? week)
        {
            if (week == null) return BadRequest();

            var sessions = week.Sessions
                .OrderBy(session => session.StartTime)
                .Select(session => $"{session.Subject} {session.StartTime.ToString("dddd", CultureInfo.InvariantCulture)} {Clock(session.StartTime)}");
            return Ok(new TrainingWeekConfirmed($"Week confirmed: {string.Join(", ", sessions)}."));
        }

        private static string Clock(DateTimeOffset time) => time.ToString("h:mm tt", CultureInfo.InvariantCulture);
    }
}
