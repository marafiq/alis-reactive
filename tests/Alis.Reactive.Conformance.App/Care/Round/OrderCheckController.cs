using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class OrderCheckController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.OrderCheck)]
    public IActionResult OfTheRound() => View("/Care/Round/OrderCheck.cshtml", new RoundOrder());

    [HttpPost(CareEndpoints.OrderCheckOrders)]
    public IActionResult Save([FromForm] RoundOrder order)
    {
        var validation = this.Validate<RoundOrder, RoundValidator>(order);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return Content("Order saved", PlainText);
    }
}

public sealed class RoundOrder
{
    public int Strength { get; set; }
}

public sealed class RoundValidator : ReactiveValidator<RoundOrder>
{
    private const int WardLimit = 500;

    public RoundValidator() => RuleFor(order => order.Strength).Must(strength => strength <= WardLimit).WithMessage("Strength is above the ward's limit.");
}
