using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Training calendar: November 2026 staff training, opened on Thursday the 5th. A session's card opens its
    /// details in the drawer, and Agenda lists the sessions of the week that was on screen.
    /// </summary>
    public partial class ScheduleController
    {
        private static readonly IReadOnlyList<TrainingCalendarSession> TrainingSessions =
        [
            new(1, "Fire safety drill", "Dana Whitfield", new DateTime(2026, 11, 5, 14, 0, 0), new DateTime(2026, 11, 5, 15, 0, 0)),
            new(2, "Medication safety refresher", "Priya Raman", new DateTime(2026, 11, 9, 10, 0, 0), new DateTime(2026, 11, 9, 11, 0, 0)),
            new(3, "Dementia care workshop", "Luis Ortega", new DateTime(2026, 11, 13, 13, 0, 0), new DateTime(2026, 11, 13, 15, 0, 0)),
        ];

        [HttpGet("TrainingCalendar")]
        public IActionResult TrainingCalendar() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/Schedule/TrainingCalendar.cshtml", new TrainingCalendarPageModel());

        [HttpGet("TrainingCalendar/Sessions")]
        public IActionResult TrainingCalendarSessions() =>
            Ok(new TrainingCalendarSessions(TrainingSessions, $"{TrainingSessions.Count} training sessions this month."));

        [HttpGet("TrainingCalendar/Details")]
        public IActionResult TrainingCalendarDetails(int sessionId) =>
            PartialView("~/Areas/Sandbox/Views/Components/Fusion/Schedule/_TrainingSessionDetails.cshtml",
                TrainingSessions.Single(session => session.Id == sessionId));
    }
}
