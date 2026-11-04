using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.ResidentAdmission;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns.Workflows
{
    [Area("Sandbox")]
    [Route("Sandbox/Patterns/BddExperiment")]
    public class BddExperimentController : Controller
    {
        private static readonly CultureInfo Dollars = CultureInfo.GetCultureInfo("en-US");

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Patterns/BddExperiment/Index.cshtml", new AdmitResidentModel());

        [HttpPost("Submit")]
        public IActionResult Submit([FromBody] AdmitResidentModel? admission)
        {
            if (admission == null) return BadRequest();

            var result = new AdmitResidentValidator().Validate(admission);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new ResidentAdmitted(Confirmation(admission)));
        }

        private static string Confirmation(AdmitResidentModel admission)
        {
            var admittedTo = string.Concat(AdmissionCareLevel.All
                .Where(level => level.Code == admission.CareLevel)
                .Select(level => $" to {level.Name}"));
            var moveIn = admission.MovesInToday ? "moving in today" : "moving in on the scheduled day";
            var rate = admission.MonthlyRate.GetValueOrDefault().ToString("C", Dollars);
            return $"{admission.ResidentName} is admitted{admittedTo} under {admission.Physician} at {rate} a month, {moveIn}.";
        }
    }
}
