using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class TableSupportController : Controller
{
    [HttpGet(CarePages.TableSupport)]
    public IActionResult OfTableFour() => View("/Care/Residents/TableSupport.cshtml", new TableSupport(4, TheSeats()));

    private static List<SupportSeat> TheSeats()
        => [new("Ada Hart", "close"), new("Ben Okoro", "standard", UsesAWheelchair: true), new("Cora Diaz", "close"), new("Dev Patel", null)];
}

public sealed record TableSupport(int Table, IReadOnlyList<SupportSeat> Seats);

public sealed record SupportSeat(string Resident, string? Supervision, bool UsesAWheelchair = false);
