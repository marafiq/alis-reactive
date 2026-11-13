using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitScheduleController : Controller
{
    [HttpGet(CarePages.VisitSchedule)]
    public IActionResult ForAResident() => View("/Care/Admission/VisitSchedule.cshtml", new VisitSchedule { VisitDays = ["Mon"] });
}

public sealed class VisitSchedule
{
    public List<string> VisitDays { get; set; } = [];
}

public static class VisitScheduleButtons
{
    public static readonly AppEvent TuesdayAndWednesday = AppEvent.Declare();
}
