using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LeastDoseRoundController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.LeastDoseRound)]
    public IActionResult OfTheRound() => View("/Care/Round/LeastDoseRound.cshtml", new LeastDoseRound { Amounts = [0, 0] });

    [HttpPost(CareEndpoints.LeastDoseRounds)]
    public IActionResult Save([FromForm] LeastDoseRound round)
    {
        var validation = this.Validate<LeastDoseRound, LeastDoseRoundRules>(round);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content(string.Join(", ", round.Amounts.Select(amount => amount.ToString(CultureInfo.InvariantCulture))), PlainText);
    }
}

public sealed class LeastDoseRound
{
    public int Least { get; set; }

    public List<int> Amounts { get; set; } = [];
}

public sealed class LeastDoseRoundRules : ReactiveValidator<LeastDoseRound>
{
    public LeastDoseRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(round => round.Least).WithMessage("Give at least the least dose.");
}
