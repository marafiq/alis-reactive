using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentIntakeController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ResidentIntake)]
    public IActionResult OfAResident() => View("/Care/Admission/ResidentIntake.cshtml", new ResidentIntake());

    [HttpPost(ResidentIntakeEndpoints.Intakes)]
    public IActionResult Save([FromForm] ResidentIntake intake) => Content($"Saved {intake.Name}", PlainText);
}

public sealed class ResidentIntake
{
    public string? Name { get; set; }
}

public sealed class ResidentIntakeRules : ReactiveValidator<ResidentIntake>
{
    public ResidentIntakeRules() => RuleFor(intake => intake.Name).NotEmpty().WithMessage("Name the resident.");
}

public static class ResidentIntakeEndpoints
{
    public const string Intakes = CarePages.Area + "admission/resident-intake/intakes";
}

public static class ResidentIntakeButtons
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
