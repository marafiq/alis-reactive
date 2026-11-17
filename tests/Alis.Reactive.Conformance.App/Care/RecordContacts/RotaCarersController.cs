using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RotaCarersController : Controller
{
    [HttpGet(CarePages.RotaCarers)]
    public IActionResult OnShift() => View("/Care/RecordContacts/RotaCarers.cshtml", new RotaCarers(new DateTime(2026, 11, 9), Shifts()));

    private static IReadOnlyList<CarerShift> Shifts()
    {
        var ada = new CarerShift("Ada Brooks", new DateTime(2026, 11, 9, 9, 0, 0), new DateTime(2026, 11, 9, 17, 0, 0));
        var lena = new CarerShift("Lena Ortiz", new DateTime(2026, 11, 10, 9, 0, 0), new DateTime(2026, 11, 10, 17, 0, 0));
        return [ada, lena];
    }
}

public sealed record RotaCarers(DateTime SelectedDate, IReadOnlyList<CarerShift> Shifts);

public sealed record CarerShift(string Carer, DateTime Starts, DateTime Ends);
