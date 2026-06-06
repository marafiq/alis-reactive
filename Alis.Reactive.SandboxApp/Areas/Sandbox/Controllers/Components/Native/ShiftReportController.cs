using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// ShiftReport sandbox — the array DSL operating on a CUSTOM event payload. A button loads the
    /// alert roster over HTTP, then broadcasts it as the <c>shift-report</c> custom event whose
    /// payload carries <c>ResidentAlert[]</c>; the listener runs filter/aggregate/find over the
    /// payload's element members. URL: /Sandbox/Components/ShiftReport.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/ShiftReport")]
    public class ShiftReportController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Native/ShiftReport/Index.cshtml",
                new ShiftReportModel());
        }

        /// <summary>Returns the alert array dispatched as the custom event payload.</summary>
        [HttpGet("Alerts")]
        public IActionResult Alerts()
        {
            return Ok(new AlertsResponse
            {
                Alerts = new[]
                {
                    new ResidentAlert { Resident = "Maple", Severity = "critical", Priority = 9, Acknowledged = false },
                    new ResidentAlert { Resident = "Birch", Severity = "stable",   Priority = 2, Acknowledged = true },
                    new ResidentAlert { Resident = "Cedar", Severity = "critical", Priority = 7, Acknowledged = true },
                    new ResidentAlert { Resident = "Aspen", Severity = "urgent",   Priority = 5, Acknowledged = false },
                    new ResidentAlert { Resident = "Oak",   Severity = "critical", Priority = 4, Acknowledged = false },
                },
            });
        }

        /// <summary>Returns the all-clear alert array used by the custom-event Else guard.</summary>
        [HttpGet("AlertsAllClear")]
        public IActionResult AlertsAllClear()
        {
            return Ok(new AlertsResponse
            {
                Alerts = new[]
                {
                    new ResidentAlert { Resident = "Maple", Severity = "critical", Priority = 9, Acknowledged = true },
                    new ResidentAlert { Resident = "Birch", Severity = "stable",   Priority = 2, Acknowledged = true },
                    new ResidentAlert { Resident = "Cedar", Severity = "critical", Priority = 7, Acknowledged = true },
                    new ResidentAlert { Resident = "Aspen", Severity = "urgent",   Priority = 5, Acknowledged = true },
                    new ResidentAlert { Resident = "Oak",   Severity = "critical", Priority = 4, Acknowledged = true },
                },
            });
        }
    }
}
