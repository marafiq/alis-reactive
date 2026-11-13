using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeAndKinController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.IntakeAndKin)]
    public IActionResult OfAnApplicant() => View("/Care/Admission/IntakeAndKin.cshtml", new IntakeAndKin());

    [HttpGet(IntakeAndKinEndpoints.Line)]
    public IActionResult Line() => PartialView("/Care/Admission/IntakeAndKinLine.cshtml", new IntakeAndKin());

    [HttpPost(IntakeAndKinEndpoints.Intakes)]
    public IActionResult SaveIntake([FromForm] IntakeAndKin intake)
    {
        if (string.IsNullOrEmpty(intake.Bed)) ModelState.AddModelError(nameof(IntakeAndKin.Bed), "Choose a bed.");
        if (ModelState.IsValid) return Content("Intake saved", PlainText);
        return ValidationProblem(ModelState);
    }

    [HttpPost(IntakeAndKinEndpoints.NextOfKin)]
    public IActionResult SaveNextOfKin([FromForm] IntakeAndKin nextOfKin)
    {
        if (string.IsNullOrEmpty(nextOfKin.Relationship)) ModelState.AddModelError(nameof(IntakeAndKin.Relationship), "Give the relationship.");
        if (ModelState.IsValid) return Content("Next of kin saved", PlainText);
        return ValidationProblem(ModelState);
    }
}

public sealed class IntakeAndKin
{
    public string? Resident { get; set; }

    public string? Note { get; set; }

    public string? Bed { get; set; }

    public string? Kin { get; set; }

    public string? Relationship { get; set; }
}

public static class IntakeAndKinEndpoints
{
    public const string Line = CarePages.Area + "admission/intake-and-kin/line";

    public const string Intakes = CarePages.Area + "admission/intake-and-kin/intakes";

    public const string NextOfKin = CarePages.Area + "admission/intake-and-kin/next-of-kin";
}

public static class IntakeAndKinButtons
{
    public static readonly AppEvent OpenTheLine = AppEvent.Declare();

    public static readonly AppEvent CloseTheLine = AppEvent.Declare();

    public static readonly AppEvent SaveIntake = AppEvent.Declare();

    public static readonly AppEvent SaveNextOfKin = AppEvent.Declare();
}
