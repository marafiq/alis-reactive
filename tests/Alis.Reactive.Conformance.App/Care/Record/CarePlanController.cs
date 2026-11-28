using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarePlanController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.CarePlan)]
    public IActionResult OfAResident() => View("/Care/Record/CarePlan.cshtml", new CarePlan());

    [HttpPost(CarePlanEndpoints.Plans)]
    public IActionResult Save([FromForm] CarePlan carePlan) => Content($"Allergies saved: {carePlan.Allergies}", PlainText);
}

public sealed class CarePlan
{
    public string? PreferredName { get; set; }

    public string? Allergies { get; set; }
}

public sealed class CarePlanRules : ReactiveValidator<CarePlan>
{
    public CarePlanRules() => RuleFor(carePlan => carePlan.Allergies).NotEmpty().WithMessage("List allergies or none.");
}

public static class CarePlanEndpoints
{
    public const string Plans = CarePages.CarePlan + "/plans";
}
