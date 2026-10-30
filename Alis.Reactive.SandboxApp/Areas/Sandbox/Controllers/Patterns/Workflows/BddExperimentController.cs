using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.ResidentAdmission;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns.Workflows
{
    /// <summary>
    /// Admitting a new resident: the server checks the admission with the page's validator, including the
    /// check only it can make, and confirms it in its own words.
    /// </summary>
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

        // "Margaret Thompson is admitted to Assisted Living under Alan Smith, MD at $4,500.00 a month, moving in today."
        private static string Confirmation(AdmitResidentModel admission)
        {
            // " to Assisted Living", or nothing while no care level is chosen.
            var admittedTo = string.Concat(AdmissionCareLevel.All
                .Where(level => level.Code == admission.CareLevel)
                .Select(level => $" to {level.Name}"));
            var moveIn = admission.MovesInToday ? "moving in today" : "moving in on the scheduled day";
            var rate = admission.MonthlyRate.GetValueOrDefault().ToString("C", Dollars);
            return $"{admission.ResidentName} is admitted{admittedTo} under {admission.Physician} at {rate} a month, {moveIn}.";
        }
    }
}
