using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundCensusController : Controller
{
    [HttpGet(CarePages.RoundCensus)]
    public IActionResult Census() => View("/Care/Round/RoundCensus.cshtml", new RoundCensus("North"));

    [HttpGet(CareEndpoints.RoundCensusCount)]
    [AtTheRespondGate]
    public IActionResult Count()
    {
        World.Heard(Request.Headers.TraceParent, Request.Headers.Baggage);
        var status = World.NextAnswer(CareEndpoints.RoundCensusCount);
        return status == StatusCodes.Status200OK ? Json(new WardCount("12")) : StatusCode(status);
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed record RoundCensus(string Ward);

public sealed record WardCount(string Count);

public static class RoundCensusEvents
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
