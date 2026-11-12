using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FlaggedAlarmController : Controller
{
    [HttpGet(CarePages.FlaggedAlarm)]
    public IActionResult OfAResident() => View("/Care/Record/FlaggedAlarm.cshtml", TheAlarm());

    [HttpGet(CarePages.FlaggedAlarmWrittenLate)]
    public IActionResult WrittenLate() => View("/Care/Record/FlaggedAlarmWrittenLate.cshtml", TheAlarm());

    [HttpGet(CarePages.FlaggedAlarmWrittenFirst)]
    public IActionResult WrittenFirst() => View("/Care/Record/FlaggedAlarmWrittenFirst.cshtml", TheAlarm());

    private static FlaggedAlarm TheAlarm() => new("Ada Hart", "Bed alarm");
}

public sealed record FlaggedAlarm(string Resident, string Alarm);
