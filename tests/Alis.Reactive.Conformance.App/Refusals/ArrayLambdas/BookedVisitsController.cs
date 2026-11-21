using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class BookedVisitsController : Controller
{
    [HttpGet(RefusalPages.BookedVisits)]
    public IActionResult Index() => View("/Refusals/ArrayLambdas/BookedVisits.cshtml");

    [HttpGet(RefusalPages.BookedVisitCount)]
    public IActionResult Count() => View("/Refusals/ArrayLambdas/BookedVisitCount.cshtml", new VisitCutoff(new DateTime(2026, 10, 31)));

    [HttpGet(BookedVisitEndpoints.Visits)]
    public IActionResult Visits() => Json(new List<BookedVisit>
    {
        new("Ada Hart", new DateTime(2026, 10, 27, 14, 0, 0)),
        new("Ben Ode", new DateTime(2026, 10, 29, 10, 30, 0)),
        new("Cy Roe", new DateTime(2026, 11, 1, 15, 0, 0)),
        new("Di Lam", new DateTime(2026, 11, 3, 11, 0, 0)),
    });
}

public sealed record VisitCutoff(DateTime Cutoff);

public sealed record BookedVisit(string Visitor, DateTime At);

public static class BookedVisitCounts
{
    public static readonly AppEvent Count = AppEvent.Declare();
}

public static class BookedVisitEndpoints
{
    public const string Visits = RefusalPages.BookedVisits + "/visits";
}
