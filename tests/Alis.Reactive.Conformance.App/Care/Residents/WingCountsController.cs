using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WingCountsController : Controller
{
    [HttpGet(CarePages.WingCounts)]
    public IActionResult OfAWing() => View("/Care/Residents/WingCounts.cshtml", new WingCounts("North wing"));

    [HttpGet(CareEndpoints.WingCountsResidents)]
    public async Task<IActionResult> Residents()
    {
        await World.Reach(GateName.Of(LanesGate.Settle), Request.Path);
        return NoContent();
    }

    [HttpGet(CareEndpoints.WingCountsDischarges)]
    public async Task<IActionResult> Discharges()
    {
        await World.Reach(GateName.Of(LanesGate.Settle), Request.Path);
        return NoContent();
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed record WingCounts(string Wing);

public static class WingCountsButtons
{
    public static readonly AppEvent Refresh = AppEvent.Declare();
}
