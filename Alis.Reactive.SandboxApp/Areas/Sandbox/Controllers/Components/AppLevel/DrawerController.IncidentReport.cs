using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    /// <summary>
    /// Incident reports: the nurse files a report from the drawer. Filing takes a moment, during
    /// which a loader covers the form; the nurse may close the drawer before it answers.
    /// </summary>
    public partial class DrawerController
    {
        private static readonly TimeSpan FilingTime = TimeSpan.FromMilliseconds(1200);

        [HttpGet("IncidentReport")]
        public IActionResult IncidentReport() =>
            View("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/IncidentReport.cshtml", new IncidentReportPageModel());

        [HttpGet("IncidentReport/Form")]
        public IActionResult IncidentReportForm() =>
            PartialView("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/_IncidentReportPartial.cshtml",
                new IncidentReportModel());

        [HttpPost("IncidentReport/File")]
        public async Task<IActionResult> FileIncidentReport([FromBody] IncidentReportModel? report)
        {
            if (report == null) return BadRequest();

            var result = new IncidentReportValidator().Validate(report);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            await Task.Delay(FilingTime);
            return Ok(new IncidentReportFiled($"Incident report filed: {report.Summary}"));
        }
    }
}
