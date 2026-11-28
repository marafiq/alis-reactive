using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FluidBalanceController : Controller
{
    [HttpGet(CarePages.FluidBalanceMeasuredBeforeItsWrite)]
    public IActionResult MeasuredBeforeItsWrite() => View("/Care/Record/FluidBalanceMeasuredBeforeItsWrite.cshtml", new FluidBalance("Ada Hart"));

    [HttpGet(CarePages.FluidBalanceMeasuredAfterItsWrite)]
    public IActionResult MeasuredAfterItsWrite() => View("/Care/Record/FluidBalanceMeasuredAfterItsWrite.cshtml", new FluidBalance("Ada Hart"));
}

public sealed record FluidBalance(string Resident);

public static class FluidBalances
{
    public static readonly AppEvent Check = AppEvent.Declare();

    public static readonly AppEvent Restore = AppEvent.Declare();
}
