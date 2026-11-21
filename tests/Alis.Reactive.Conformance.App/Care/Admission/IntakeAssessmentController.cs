using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeAssessmentController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.IntakeAssessment)]
    public IActionResult OfAResident() => View("/Care/Admission/IntakeAssessment.cshtml", new IntakeAssessment());

    [HttpPost(IntakeAssessmentEndpoints.Intakes)]
    public IActionResult Save([FromForm] IntakeAssessment _)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        return Content("Saved", PlainText);
    }
}

public sealed class IntakeAssessment
{
    public decimal Strength { get; set; }
}

public sealed class IntakeAssessmentRules : ReactiveValidator<IntakeAssessment>
{
    public IntakeAssessmentRules() => RuleFor(intake => intake.Strength).GreaterThan(1m).WithMessage("Give a strength above 1.");
}

public static class IntakeAssessmentEndpoints
{
    public const string Intakes = CarePages.IntakeAssessment + "/intakes";

    public const string IntakesInGerman = Intakes + "?culture=de-DE";
}
