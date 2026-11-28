using System.Globalization;
using System.Web.Mvc;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LeastDoseRoundController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(CarePages.LeastDoseRound)]
    public ActionResult OfTheRound() => View("/Care/Round/LeastDoseRound.cshtml", new LeastDoseRound { Amounts = [0, 0] });

    [HttpPost]
    [Address(CareEndpoints.LeastDoseRounds)]
    public ActionResult Save(LeastDoseRound round)
    {
        var validation = this.Validate<LeastDoseRound, LeastDoseRoundRules>(round);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content(string.Join(", ", round.Amounts.Select(amount => amount.ToString(CultureInfo.InvariantCulture))), PlainText);
    }
}
