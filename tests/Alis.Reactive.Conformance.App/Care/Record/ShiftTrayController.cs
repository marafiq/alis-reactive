using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftTrayController : Controller
{
    [HttpGet(CarePages.ShiftTray)]
    public IActionResult OfAResident() => View("/Care/Record/ShiftTray.cshtml", new ShiftTray("Ada Hart"));

    [HttpGet(ShiftTrayEndpoints.Tools)]
    public IActionResult Tools() => PartialView("/Care/Record/ShiftTrayTools.cshtml", new ShiftTrayTools("Night shift"));
}

public sealed record ShiftTray(string Resident);

public sealed record ShiftTrayTools(string Shift);

public static class ShiftTrayEndpoints
{
    private const string Root = CarePages.Area + "record/shift-tray/";

    public const string Tools = Root + "tools";
}

public static class ShiftTrays
{
    public static readonly AppEvent OpenTheTray = AppEvent.Declare();

    public static readonly AppEvent ClearTheTray = AppEvent.Declare();

    public static readonly AppEvent MarkTheShift = AppEvent.Declare();
}
