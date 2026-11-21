using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusRefreshController : Controller
{
    [HttpGet(CarePages.CensusRefresh)]
    public IActionResult Refresh() => View("/Care/Residents/CensusRefresh.cshtml", new CensusRefresh("North wing"));

    [HttpGet(CareEndpoints.CensusFromTheFirstSource)]
    public async Task<IActionResult> FromTheFirstSource()
    {
        await World.Reach(GateName.Of(LanesGate.Settle), Request.Path);
        return NoContent();
    }

    [HttpGet(CareEndpoints.CensusFromTheSecondSource)]
    public async Task<IActionResult> FromTheSecondSource()
    {
        await World.Reach(GateName.Of(LanesGate.Settle), Request.Path);
        return NoContent();
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed record CensusRefresh(string Wing);

public static class CensusRefreshes
{
    public static readonly AppEvent Refresh = AppEvent.Declare();
}
