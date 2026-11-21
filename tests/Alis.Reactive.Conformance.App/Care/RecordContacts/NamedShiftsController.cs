using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NamedShiftsController : Controller
{
    [HttpGet(CarePages.NamedShifts)]
    public IActionResult OnShift() => View("/Care/RecordContacts/NamedShifts.cshtml", new NamedShifts(new DateTime(2026, 11, 9), Shifts()));

    private static IReadOnlyList<NamedShift> Shifts()
    {
        var ada = new NamedShift("Ada Brooks", new DateTime(2026, 11, 9, 9, 0, 0), new DateTime(2026, 11, 9, 17, 0, 0));
        var lena = new NamedShift("Lena Ortiz", new DateTime(2026, 11, 10, 9, 0, 0), new DateTime(2026, 11, 10, 17, 0, 0));
        return [ada, lena];
    }
}

public sealed record NamedShifts(DateTime SelectedDate, IReadOnlyList<NamedShift> Shifts);

public sealed record NamedShift(
    [property: JsonPropertyName("who")] string Carer,
    [property: JsonPropertyName("from")] DateTime Starts,
    [property: JsonPropertyName("until")] DateTime Ends);
