using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusDaysController : Controller
{
    [HttpGet(CarePages.CensusDaysWithADayHeldAsSlashedText)]
    public IActionResult WithADayHeldAsSlashedText() => AdmittedWithLeeOn("10/27/2026");

    [HttpGet(CarePages.CensusDaysWithEveryDayHeldAsADate)]
    public IActionResult WithEveryDayHeldAsADate() => AdmittedWithLeeOn("2026-10-27");

    [HttpGet(CarePages.CensusDaysWithADayNoMonthHolds)]
    public IActionResult WithADayNoMonthHolds() => AdmittedWithLeeOn("2026-02-30");

    private ViewResult AdmittedWithLeeOn(string day) => View("/Care/Residents/CensusDays.cshtml", new CensusDays(
        new DateTime(2026, 11, 1),
        [new RecordStart { Name = "Martin", Day = "2026-10-20" }, new RecordStart { Name = "Lee", Day = day }, new RecordStart { Name = "Adams", Day = "2026-11-05" }]));
}

public sealed record CensusDays(DateTime Cutoff, IReadOnlyList<RecordStart> Admissions);

public static class CensusDayEvents
{
    public static readonly AppEvent Count = AppEvent.Declare();
}
