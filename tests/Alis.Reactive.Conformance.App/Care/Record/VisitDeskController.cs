using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitDeskController : Controller
{
    [HttpGet(CarePages.VisitDesk)]
    public IActionResult OfAResident() => View("/Care/Record/VisitDesk.cshtml", new VisitDesk("Ada Hart"));

    [HttpGet(VisitDeskEndpoints.Tools)]
    public IActionResult Tools() => PartialView("/Care/Record/VisitDeskTools.cshtml", new VisitDeskTools("Visits today"));

    [HttpGet(VisitDeskEndpoints.Visit)]
    public IActionResult Visit() => PartialView("/Care/Record/VisitDeskVisit.cshtml", new VisitDeskVisit("Ben Hart at four"));
}

public sealed record VisitDesk(string Resident);

public sealed record VisitDeskTools(string Day);

public sealed record VisitDeskVisit(string Visitor);

public static class VisitDeskEndpoints
{
    private const string Root = CarePages.Area + "record/visit-desk/";

    public const string Tools = Root + "tools";

    public const string Visit = Root + "visit";
}

public static class VisitDesks
{
    public static readonly AppEvent OpenTheDesk = AppEvent.Declare();

    public static readonly AppEvent CloseTheDesk = AppEvent.Declare();

    public static readonly AppEvent MarkNoVisit = AppEvent.Declare();

    public static readonly AppEvent LoadTheVisit = AppEvent.Declare();

    public static readonly AppEvent CloseTheVisit = AppEvent.Declare();
}
