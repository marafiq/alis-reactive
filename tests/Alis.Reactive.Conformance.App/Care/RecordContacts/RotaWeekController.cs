using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RotaWeekController : Controller
{
    [HttpGet(CarePages.RotaWeek)]
    public IActionResult OfTheWard() => View("/Care/RecordContacts/RotaWeek.cshtml", new RotaWeek { SelectedDate = new DateTime(2026, 11, 9) });
}

public sealed class RotaWeek
{
    public DateTime SelectedDate { get; init; }
}
