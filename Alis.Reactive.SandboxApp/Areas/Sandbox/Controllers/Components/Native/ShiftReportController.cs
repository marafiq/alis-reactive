using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.ShiftReport;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/ShiftReport")]
    public class ShiftReportController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
            => View("~/Areas/Sandbox/Views/Components/Native/ShiftReport/Index.cshtml", new ShiftAlertReviewModel());

        [HttpGet("Day")]
        public IActionResult DayShift() => Ok(new ShiftAlertsResponse
        {
            Shift = "Day shift",
            Alerts = new[]
            {
                Alert("Agnes Moore", AlertSeverity.Critical, priority: 6, acknowledged: true),
                Alert("Harold Kim",  AlertSeverity.Urgent,   priority: 3, acknowledged: true),
                Alert("Frank Ortiz", AlertSeverity.Critical, priority: 8, acknowledged: true),
                Alert("Ida Bell",    AlertSeverity.Stable,   priority: 1, acknowledged: true),
            },
        });

        [HttpGet("Evening")]
        public IActionResult EveningShift() => Ok(new ShiftAlertsResponse
        {
            Shift = "Evening shift",
            Alerts = new[]
            {
                Alert("Evelyn Hart",   AlertSeverity.Urgent,   priority: 5, acknowledged: false),
                Alert("Walter Brooks", AlertSeverity.Critical, priority: 7, acknowledged: true),
                Alert("Margaret Hale", AlertSeverity.Critical, priority: 9, acknowledged: false),
                Alert("Rose Delgado",  AlertSeverity.Stable,   priority: 2, acknowledged: true),
                Alert("Robert Miles",  AlertSeverity.Critical, priority: 4, acknowledged: false),
            },
        });

        private static ShiftAlert Alert(string resident, string severity, int priority, bool acknowledged)
            => new ShiftAlert { Resident = resident, Severity = severity, Priority = priority, Acknowledged = acknowledged };
    }
}
