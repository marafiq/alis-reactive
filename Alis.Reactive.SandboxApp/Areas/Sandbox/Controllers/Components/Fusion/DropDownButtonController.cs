using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownButton;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionDropDownButton")]
    public sealed class DropDownButtonController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/DropDownButton/Index.cshtml", new FusionDropDownButtonModel());
        }

        // No incident was logged this shift, so the incident summary has nothing to print.
        [HttpPost("Print")]
        public IActionResult Print([FromBody] ShiftReportRequest request)
        {
            var report = ShiftReports.All.FirstOrDefault(r => r.Id == request.Report);
            if (report is null || report.AfterMidnightCount)
                return Ok(new ShiftReportAnswer { Message = "Choose a report from the menu." });

            if (report.Id == ShiftReports.IncidentSummary)
            {
                return Ok(new ShiftReportAnswer
                {
                    Message = $"No incidents were logged on {request.Wing} wing this shift.",
                    EmptyReportNames = [report.Name]
                });
            }

            return Ok(new ShiftReportAnswer
            {
                Printed = true,
                Message = $"Sent to the {request.Wing} nurses' station printer.",
                PrintedReportIds = [report.Id]
            });
        }
    }
}
