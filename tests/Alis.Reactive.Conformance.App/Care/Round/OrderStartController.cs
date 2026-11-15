using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class OrderStartController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.OrderStart)]
    public IActionResult Start() => View("/Care/Round/OrderStart.cshtml", new OrderStart("Aspirin 75 mg"));

    [HttpPost(OrderStartEndpoints.Orders)]
    public IActionResult Order() => Content("Ordered", PlainText);
}

public sealed record OrderStart(string Medication);

public static class OrderStarts
{
    public static readonly AppEvent Start = AppEvent.Declare();
}

public static class OrderStartEndpoints
{
    public const string Orders = CarePages.OrderStart + "/orders";
}
