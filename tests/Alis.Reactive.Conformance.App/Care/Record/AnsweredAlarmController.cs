using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AnsweredAlarmController : Controller
{
    [HttpGet(CarePages.AnsweredAlarm)]
    public IActionResult ByTwoHandlers() => View("/Care/Record/AnsweredAlarm.cshtml", TheAlarm());

    [HttpGet(CarePages.AnsweredAlarmWithOneHandler)]
    public IActionResult ByOneHandler() => View("/Care/Record/AnsweredAlarmWithOneHandler.cshtml", TheAlarm());

    private static AnsweredAlarm TheAlarm() => new("Ada Hart", "Call bell");
}

public sealed record AnsweredAlarm(string Resident, string Alarm);
