using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SupplyCountController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.SupplyCount)]
    public IActionResult OfTheCupboard() => View("/Care/Round/SupplyCount.cshtml", new SupplyCount());

    [HttpPost(SupplyCountEndpoints.Counts)]
    public IActionResult Save([FromForm] SupplyCount supply) => Content(supply.Count.ToString(CultureInfo.InvariantCulture), PlainText);
}

public sealed class SupplyCount
{
    public int Count { get; set; }
}

public sealed class SupplyCountRules : ReactiveValidator<SupplyCount>
{
    public SupplyCountRules() => RuleFor(supply => supply.Count).GreaterThan(0);
}

public static class SupplyCountEndpoints
{
    public const string Counts = CarePages.SupplyCount + "/counts";
}
