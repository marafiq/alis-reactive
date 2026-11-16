using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseOrderingController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.DoseOrdering)]
    public IActionResult OfTheRound() => View("/Care/Round/DoseOrdering.cshtml", new DoseOrdering());

    [HttpPost(DoseOrderingEndpoints.Orders)]
    public IActionResult Save([FromForm] DoseOrdering order) => Content("Saved " + order.Doses.ToString(CultureInfo.InvariantCulture), PlainText);
}

public sealed class DoseOrdering
{
    public int Doses { get; set; } = 5;
}

public sealed class DoseOrderingRules : ReactiveValidator<DoseOrdering>
{
    public DoseOrderingRules() => RuleFor(order => order.Doses).GreaterThan(0);
}

public static class DoseOrderingEndpoints
{
    public const string Orders = CarePages.DoseOrdering + "/orders";
}
