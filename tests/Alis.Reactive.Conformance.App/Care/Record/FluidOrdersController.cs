using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FluidOrdersController : Controller
{
    private const int MeasuredIntake = 850;

    [HttpGet(CarePages.FluidOrders)]
    public IActionResult OfTheWard() => View("/Care/Record/FluidOrders.cshtml", new FluidOrders("Ada Hart", "Ben Okoro", MeasuredIntake));

    [HttpGet(FluidOrderEndpoints.RestrictedOrder)]
    public IActionResult TheRestrictedOrder() => Json(new FluidOrder("Ada Hart", new FluidRestriction(true)));

    [HttpGet(FluidOrderEndpoints.UnrestrictedOrder)]
    public IActionResult TheUnrestrictedOrder() => Json(new FluidOrder("Ben Okoro", null));
}

public sealed record FluidOrders(string Restricted, string Unrestricted, int MeasuredIntake);

public sealed record FluidOrder(string Resident, FluidRestriction? Restriction);

public sealed record FluidRestriction(bool Applies);

public static class FluidOrderEndpoints
{
    private const string Root = CarePages.Area + "record/fluid-orders/";

    public const string RestrictedOrder = Root + "restricted/order";

    public const string UnrestrictedOrder = Root + "unrestricted/order";
}

public static class FluidOrderEvents
{
    public static readonly AppEvent SetTheRestrictedTarget = AppEvent.Declare();

    public static readonly AppEvent SetTheUnrestrictedTarget = AppEvent.Declare();
}
