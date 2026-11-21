using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedMonitorController : Controller
{
    [HttpGet(CarePages.BedMonitor)]
    public IActionResult OfAResident() => View("/Care/Record/BedMonitor.cshtml", new BedMonitor("Ada Hart", [RecordScripts.PageTally]));

    [HttpGet(BedMonitorEndpoints.Card)]
    public IActionResult Card() => PartialView("/Care/Record/BedMonitorCard.cshtml", new BedMonitorCard("Bed monitor", "Bed 4"));
}

public sealed record BedMonitor(string Resident, IReadOnlyList<string> Scripts) : IScriptedRecordPage;

public sealed record BedMonitorCard(string Heading, string Bed);

public static class BedMonitorEndpoints
{
    public const string Card = CarePages.Area + "record/bed-monitor/card";
}

public static class BedMonitorButtons
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();
}
