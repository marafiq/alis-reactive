using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Admission assessments: Save takes an assessment the page checked first; Print takes the
    /// assessment as it is and runs the same validator, so the server's own verdict shows on the page.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/AdmissionAssessment")]
    public class AdmissionAssessmentController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Validation/AdmissionAssessment/Index.cshtml", new AdmissionAssessmentModel());

        [HttpPost("Save")]
        public IActionResult Save([FromBody] AdmissionAssessmentModel? assessment) =>
            Accept(assessment, "saved");

        [HttpPost("Print")]
        public IActionResult Print([FromBody] AdmissionAssessmentModel? assessment) =>
            Accept(assessment, "printed");

        private IActionResult Accept(AdmissionAssessmentModel? assessment, string outcome)
        {
            if (assessment == null) return BadRequest();

            var result = new AdmissionAssessmentValidator().Validate(assessment);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new AdmissionAssessmentConfirmation($"Assessment {outcome} for {assessment.ResidentName}."));
        }
    }
}
