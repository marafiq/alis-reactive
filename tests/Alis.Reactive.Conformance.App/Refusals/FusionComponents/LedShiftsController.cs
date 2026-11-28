using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class LedShiftsController : Controller
{
    private const string Shifts = "/Refusals/FusionComponents/LedShifts.cshtml";

    [HttpGet(RefusalPages.LedShiftsByLead)]
    public IActionResult ByLead() => View(Shifts, new LedShifts(true, new DateTime(2026, 11, 9), Monday()));

    [HttpGet(RefusalPages.LedShiftsByCarer)]
    public IActionResult ByCarer() => View(Shifts, new LedShifts(false, new DateTime(2026, 11, 9), Monday()));

    private static IReadOnlyList<LedShift> Monday()
        => [new LedShift("Ada Brooks", new DateTime(2026, 11, 9, 9, 0, 0), new DateTime(2026, 11, 9, 17, 0, 0), new ShiftLead("Rose Achebe"))];
}

public sealed record LedShifts(bool ByLead, DateTime SelectedDate, IReadOnlyList<LedShift> Shifts);

public sealed record LedShift(string Carer, DateTime Starts, DateTime Ends, ShiftLead Lead);

public sealed record ShiftLead(string Name);
