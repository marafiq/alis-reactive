using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CountedAlarmController : Controller
{
    [HttpGet(CarePages.CountedAlarm)]
    public IActionResult OfAResident() => View("/Care/Record/CountedAlarm.cshtml", TheAlarm());

    [HttpGet(CarePages.CountedAlarmWrittenAfterAnAwaitedCheck)]
    public IActionResult WrittenAfterAnAwaitedCheck() => View("/Care/Record/CountedAlarmWrittenAfterAnAwaitedCheck.cshtml", TheAlarm());

    [HttpGet(CarePages.CountedAlarmWrittenAfterAFlagCheck)]
    public IActionResult WrittenAfterAFlagCheck() => View("/Care/Record/CountedAlarm.cshtml", TheAlarm());

    private static CountedAlarm TheAlarm() => new("Ada Hart", "Door alarm");
}

public sealed record CountedAlarm(string Resident, string Alarm);
