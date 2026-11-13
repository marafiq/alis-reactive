using System.Collections.Generic;
using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundDosingController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.RoundDosing)]
    public IActionResult OfTheRound() => View("/Care/Round/RoundDosing.cshtml", new RoundDosing { Doses = [new(), new()] });

    [HttpGet(RoundDosingEndpoints.AddedDose)]
    public IActionResult AddedDose() => PartialView("/Care/Round/RoundDosingAddedDose.cshtml", new RoundDosingAddedDose { Doses = [new(), new(), new()] });

    [HttpPost(RoundDosingEndpoints.Rounds)]
    public IActionResult Save([FromForm] RoundDosing round) => Content(round.Doses.Count.ToString(CultureInfo.InvariantCulture), PlainText);
}

public sealed class RoundDosing
{
    public List<RoundDosingDose> Doses { get; set; } = [];
}

public sealed class RoundDosingAddedDose
{
    public List<RoundDosingDose> Doses { get; set; } = [];
}

public sealed class RoundDosingDose
{
    public int Amount { get; set; }
}

public sealed class RoundDosingRules : ReactiveValidator<RoundDosing>
{
    public RoundDosingRules()
        => RuleForEach(round => round.Doses).ChildRules(dose => dose.RuleFor(given => given.Amount).GreaterThan(0).WithMessage("Give an amount."));
}

public static class RoundDosingEndpoints
{
    public const string AddedDose = CarePages.RoundDosing + "/added-dose";

    public const string Rounds = CarePages.RoundDosing + "/rounds";
}
