using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
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
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            return PartialView(Views + "_CareStep.cshtml",
                new IntakeStepsModel { ResidentName = intake.ResidentName, FamilyPhone = intake.FamilyPhone });
        }

        [HttpGet("MobilityQuestions")]
        public IActionResult MobilityQuestions([FromQuery] string? careLevel)
            => PartialView(Views + "_MobilityQuestions.cshtml", new IntakeStepsModel { CareLevel = careLevel });

        [HttpPost("Finish")]
        public IActionResult Finish([FromBody] IntakeStepsModel? intake)
        {
            if (intake == null) return BadRequest();

            var finished = new InlineValidator<IntakeStepsModel>();
            finished.Include(new IntakeResidentValidator());
            finished.Include(new IntakeCareValidator());
            var result = finished.Validate(intake);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new IntakeConfirmation($"{intake.CareLevel} intake saved for {intake.ResidentName}."));
        }

        [HttpPost("NurseReview")]
        public IActionResult NurseReview([FromBody] IntakeStepsModel? intake)
        {
            if (intake == null) return BadRequest();

            var result = new IntakeCareValidator().Validate(intake);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new IntakeConfirmation($"{intake.CareLevel} care needs sent to the nurse for review."));
        }
    }
}
