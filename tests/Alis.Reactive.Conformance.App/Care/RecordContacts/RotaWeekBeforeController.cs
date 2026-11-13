using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RotaWeekBeforeController : Controller
{
    [HttpGet(CarePages.RotaWeekBefore)]
    public IActionResult OfTheWard() => View("/Care/RecordContacts/RotaWeekBefore.cshtml", new RotaWeekBefore { SelectedDate = new DateTime(2026, 11, 9) });
}

public sealed class RotaWeekBefore
{
    public DateTime SelectedDate { get; init; }
}
