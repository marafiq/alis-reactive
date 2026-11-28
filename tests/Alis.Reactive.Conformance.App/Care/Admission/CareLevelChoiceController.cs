using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareLevelChoiceController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.CareLevelChoice)]
    public IActionResult OfAResident() => View("/Care/Admission/CareLevelChoice.cshtml", new CareLevelChoice());

    [HttpPost(CareLevelChoiceEndpoints.Choices)]
    public IActionResult Save([FromForm] CareLevelChoice choice) => Content($"Care level {choice.CareLevel}", PlainText);
}

public sealed class CareLevelChoice
{
    public string? CareLevel { get; set; }
}

public sealed class CareLevelChoiceRules : ReactiveValidator<CareLevelChoice>
{
    public CareLevelChoiceRules() => RuleFor(choice => choice.CareLevel).NotEmpty().WithMessage("Pick a care level.");
}

public static class CareLevelChoiceEndpoints
{
    public const string Choices = CarePages.CareLevelChoice + "/choices";
}
