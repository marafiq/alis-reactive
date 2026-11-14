using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HourLogController : Controller
{
    [HttpGet(CarePages.HourLog)]
    public IActionResult OfAResident() => View("/Care/Record/HourLog.cshtml", new HourLog("Ada Hart"));

    [HttpGet(HourLogEndpoints.Tools)]
    public IActionResult Tools() => PartialView("/Care/Record/HourLogTools.cshtml", new HourLogTools("Night shift"));

    [HttpGet(HourLogEndpoints.Entry)]
    public IActionResult Entry() => PartialView("/Care/Record/HourLogEntry.cshtml", new HourLogEntry("Turned at midnight."));
}

public sealed record HourLog(string Resident);

public sealed record HourLogTools(string Shift);

public sealed record HourLogEntry(string Entry);

public static class HourLogEndpoints
{
    private const string Root = CarePages.Area + "record/hour-log/";

    public const string Tools = Root + "tools";

    public const string Entry = Root + "entry";
}

public static class HourLogs
{
    public static readonly AppEvent OpenTheDesk = AppEvent.Declare();

    public static readonly AppEvent CloseTheDesk = AppEvent.Declare();

    public static readonly AppEvent NoteTheHour = AppEvent.Declare();

    public static readonly AppEvent LoadTheEntry = AppEvent.Declare();

    public static readonly AppEvent Acknowledge = AppEvent.Declare();
}
