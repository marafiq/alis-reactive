using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PharmacyCheckController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.PharmacyCheck)]
    public IActionResult Check()
        => View("/Care/Round/PharmacyCheck.cshtml", new PharmacyCheck("North", Service<SecondOrigin>().AddressBeside(Request) + WorldEndpoints.PharmacyStock + "?" + WorldKey.AtTheSecondOrigin + "=" + World.Key));

    [HttpGet(CareEndpoints.WardStock)]
    public IActionResult WardStock()
    {
        World.Data.Heard(Request.Headers.TraceParent, Request.Headers.Baggage);
        return Content(TraceEcho.Traceparent(Request, World.Key, Service<Collector>()), PlainText);
    }

    private CurrentWorld World => Service<CurrentWorld>();

    private TService Service<TService>()
        where TService : notnull
        => HttpContext.RequestServices.GetRequiredService<TService>();
}

public sealed record PharmacyCheck(string Ward, string PharmacyStock);

public static class PharmacyCheckEvents
{
    public static readonly AppEvent CheckPharmacy = AppEvent.Declare();
    public static readonly AppEvent CheckWard = AppEvent.Declare();
}
