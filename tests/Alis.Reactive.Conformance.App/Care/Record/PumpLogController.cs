using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PumpLogController : Controller
{
    [HttpGet(CarePages.PumpLogWithNoAlarm)]
    public IActionResult WithNoAlarm() => View("/Care/Record/PumpLog.cshtml", ThePumpWith(null));

    [HttpGet(CarePages.PumpLogWithAnAlarm)]
    public IActionResult WithAnAlarm() => View("/Care/Record/PumpLog.cshtml", ThePumpWith("Air in line"));

    private static PumpLog ThePumpWith(string? alarm) => new("Ada Hart", alarm, ["Air in line", "Line occluded"]);
}

public sealed record PumpLog(string Resident, string? Alarm, IReadOnlyList<string> Alarms);

public static class PumpLogs
{
    public static readonly AppEvent LogValue = AppEvent.Declare();
}
