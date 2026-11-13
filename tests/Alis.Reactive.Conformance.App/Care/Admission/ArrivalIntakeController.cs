using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalIntakeController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ArrivalIntake)]
    public IActionResult OfAResident() => View("/Care/Admission/ArrivalIntake.cshtml", new ArrivalIntake());

    [HttpPost(ArrivalIntakeEndpoints.Intakes)]
    public IActionResult Save([FromForm] ArrivalIntake intake) => Content($"{intake.Name}, {intake.Status}", PlainText);
}

public sealed class ArrivalIntake
{
    public string? Name { get; set; } = "Unnamed";

    public string? Status { get; set; } = "Expected";
}

public sealed class ArrivalIntakeRules : ReactiveValidator<ArrivalIntake>
{
    public ArrivalIntakeRules()
    {
        RuleFor(intake => intake.Status).NotEmpty();
        RuleFor(intake => intake.Name).NotEmpty().WithMessage("Name the resident.");
    }
}

public static class ArrivalIntakeEndpoints
{
    public const string Intakes = CarePages.ArrivalIntake + "/intakes";
}
