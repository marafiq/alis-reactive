using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RepeatDoseOrderController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.RepeatDoseOrder)]
    public IActionResult OfTheRound() => View("/Care/Round/RepeatDoseOrder.cshtml", new RepeatDoseOrder());

    [HttpPost(RepeatDoseOrderEndpoints.Orders)]
    public IActionResult Save([FromForm] RepeatDoseOrder order) => Content("Saved " + order.Doses.ToString(CultureInfo.InvariantCulture), PlainText);
}

public sealed class RepeatDoseOrder
{
    public int Doses { get; set; } = 5;
}

public sealed class RepeatDoseOrderRules : ReactiveValidator<RepeatDoseOrder>
{
    public RepeatDoseOrderRules() => RuleFor(order => order.Doses).GreaterThan(4).WithMessage("Give more than 4 doses.");
}

public static class RepeatDoseOrderEndpoints
{
    public const string Orders = CarePages.RepeatDoseOrder + "/orders";
}
