using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardBoardController : Controller
{
    [HttpGet(CarePages.WardBoard)]
    public IActionResult OfAWard() => View("/Care/Round/WardBoard.cshtml", new WardBoard("East"));
}

public sealed record WardBoard(string Ward);

public sealed class AlarmReport
{
    public List<WardAlarm>? Alarms { get; init; }
}

public sealed class WardAlarm
{
    public string Room { get; init; } = "";
}

public static class WardBoards
{
    public static readonly AppEvent NoList = AppEvent.Declare();
    public static readonly AppEvent NoAlarms = AppEvent.Declare();
    public static readonly AppEvent OneAlarm = AppEvent.Declare();
    public static readonly AppEvent<AlarmReport> AlarmRaised = AppEvent.Declare<AlarmReport>();
}
