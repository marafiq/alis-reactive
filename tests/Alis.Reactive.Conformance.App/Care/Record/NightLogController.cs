using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NightLogController : Controller
{
    [HttpGet(CarePages.NightLog)]
    public IActionResult OfAResident() => View("/Care/Record/NightLog.cshtml", new NightLog("Ada Hart"));

    [HttpGet(NightLogEndpoints.Tools)]
    public IActionResult Tools() => PartialView("/Care/Record/NightLogTools.cshtml", new NightLogTools("Night shift"));

    [HttpGet(NightLogEndpoints.Entry)]
    public IActionResult Entry() => PartialView("/Care/Record/NightLogEntry.cshtml", new NightLogEntry("Slept through the night."));
}

public sealed record NightLog(string Resident);

public sealed record NightLogTools(string Shift);

public sealed record NightLogEntry(string Entry);

public static class NightLogEndpoints
{
    private const string Root = CarePages.Area + "record/night-log/";

    public const string Tools = Root + "tools";

    public const string Entry = Root + "entry";
}

public static class NightLogs
{
    public static readonly AppEvent OpenTheDesk = AppEvent.Declare();

    public static readonly AppEvent CloseTheDesk = AppEvent.Declare();

    public static readonly AppEvent MarkNoEntry = AppEvent.Declare();

    public static readonly AppEvent LoadTheEntry = AppEvent.Declare();

    public static readonly AppEvent CloseTheEntry = AppEvent.Declare();

    public static readonly AppEvent Acknowledge = AppEvent.Declare();
}
