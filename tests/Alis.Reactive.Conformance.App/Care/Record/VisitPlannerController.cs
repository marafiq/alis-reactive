using System;
using System.Collections.Generic;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitPlannerController : Controller
{
    [HttpGet(CarePages.VisitPlanner)]
    public IActionResult OfAResident()
    {
        var starts = new DateTime(2026, 11, 9, 9, 0, 0);
        var arrives = new DateTimeOffset(2026, 11, 9, 9, 0, 0, TimeSpan.FromHours(-5));
        return View("/Care/Record/VisitPlanner.cshtml", new VisitPlanner("Ada Hart", "Ruth Hart", starts, [starts, starts], null, arrives));
    }
}

public sealed record VisitPlanner(string Resident, string Visitor, DateTime Starts, List<DateTime> Days, DateTime? Ends, DateTimeOffset Arrives);

public static class VisitPlanners
{
    public static readonly AppEvent TakeTheWeek = AppEvent.Declare();

    public static readonly AppEvent LogText = AppEvent.Declare();
}
