using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RotaWeekShiftsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.RotaWeekShifts)]
    public IActionResult OfTheWard() => View("/Care/RecordContacts/RotaWeekShifts.cshtml", new RotaWeekShifts { SelectedDate = new DateTime(2026, 11, 9) });

    [HttpPost(RotaWeekShiftsEndpoints.Shifts)]
    public IActionResult Shifts([FromBody] RotaWeekQuery week) => Content($"Shifts of the week of {week.CurrentDate:d MMMM}", PlainText);
}

public sealed class RotaWeekShifts
{
    public DateTime SelectedDate { get; init; }
}

public sealed class RotaWeekQuery
{
    public DateTime CurrentDate { get; set; }
}

public static class RotaWeekShiftsEndpoints
{
    public const string Shifts = CarePages.RotaWeekShifts + "/shifts";
}
