using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// A resident intake taken in steps: the page holds one form, and each step is a partial the
    /// server returns into it once the step before is accepted.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/IntakeSteps")]
    public class IntakeStepsController : Controller
    {
        private const string Views = "~/Areas/Sandbox/Views/Validation/IntakeSteps/";

        [HttpGet("")]
        public IActionResult Index() => View(Views + "Index.cshtml", new IntakeStepsModel());

        [HttpGet("ResidentStep")]
        public IActionResult ResidentStep() => PartialView(Views + "_ResidentStep.cshtml", new IntakeStepsModel());

        [HttpPost("Resident")]
        public IActionResult Resident([FromBody] IntakeStepsModel? intake)
        {
            if (intake == null) return BadRequest();

            var result = new IntakeResidentValidator().Validate(intake);
            if (!result.IsValid) return BadRequest(new { errors = ByField(result) });

            return PartialView(Views + "_CareStep.cshtml",
                new IntakeStepsModel { ResidentName = intake.ResidentName, FamilyPhone = intake.FamilyPhone });
        }

        /// <summary>The care step's mobility questions, which depend on the care level chosen.</summary>
        [HttpGet("MobilityQuestions")]
        public IActionResult MobilityQuestions([FromQuery] string? careLevel) =>
            PartialView(Views + "_MobilityQuestions.cshtml", new IntakeStepsModel { CareLevel = careLevel });

        [HttpPost("Finish")]
        public IActionResult Finish([FromBody] IntakeStepsModel? intake)
        {
            if (intake == null) return BadRequest();

            // The finished intake answers to both steps' rules, whichever step is on the page.
            var result = new ValidationResult(
                new IntakeResidentValidator().Validate(intake).Errors
                    .Concat(new IntakeCareValidator().Validate(intake).Errors));
            if (!result.IsValid) return BadRequest(new { errors = ByField(result) });

            return Ok(new IntakeConfirmation($"{intake.CareLevel} intake saved for {intake.ResidentName}."));
        }

        private static Dictionary<string, string[]> ByField(ValidationResult result) =>
            result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
    }
}
