using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseSaveController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.DoseSave)]
    public IActionResult Round() => View("/Care/Round/DoseSave.cshtml", new DoseSave("Ruth Ellis", "Paracetamol"));

    [HttpPost(CareEndpoints.DoseSaveDoses)]
    [AtTheRespondGate]
    public IActionResult Save()
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Key;
        var collector = HttpContext.RequestServices.GetRequiredService<Collector>();
        return Content(TraceEcho.Traceparent(Request, world, collector) + ", " + TraceEcho.Baggage(Request), PlainText);
    }
}

public sealed record DoseSave(string Resident, string Medicine);

public static class DoseSaveEvents
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
