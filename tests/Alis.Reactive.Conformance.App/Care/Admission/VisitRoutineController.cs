using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitRoutineController : Controller
{
    [HttpGet(CarePages.VisitRoutine)]
    public IActionResult ForAResident() => View("/Care/Admission/VisitRoutine.cshtml", new VisitRoutine());

    [HttpPost(CarePages.VisitRoutine)]
    public IActionResult Save([FromForm] VisitRoutine routine) => View("/Care/Admission/VisitRoutineSaved.cshtml", new VisitRoutineSaved(DaysBound(routine.VisitDays)));

    private static string DaysBound(string? days) => days is null ? "No visit days" : $"Visit days: {days}";
}

public sealed class VisitRoutine
{
    public string? VisitDays { get; set; }
}

public sealed record VisitRoutineSaved(string DaysBound);
