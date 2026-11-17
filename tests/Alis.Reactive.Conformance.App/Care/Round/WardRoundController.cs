using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardRoundController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.WardRound)]
    public IActionResult OfTheWard(string ward) => View("/Care/Round/WardRound.cshtml", new WardRoundDose { Ward = ward });

    [HttpPost(CareEndpoints.WardDoses)]
    public IActionResult Save([FromForm] WardRoundDose dose)
    {
        var validation = this.Validate<WardRoundDose, WardRoundDoseRules>(dose);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content("Dose saved", PlainText);
    }

    [HttpGet(CareEndpoints.WardCensus)]
    public IActionResult Census() => Json(new WardHeadcount("12"));
}

public sealed class WardRoundDose
{
    public string Ward { get; set; } = "";

    public string Name { get; set; } = "";
}

public sealed class WardCensusAsk
{
    public string Ward { get; set; } = "";
}

public sealed record WardHeadcount(string Count);

public sealed class WardRoundDoseRules : ReactiveValidator<WardRoundDose>
{
    public WardRoundDoseRules() => RuleFor(dose => dose.Name).NotEmpty().WithMessage("Name is required.");
}
