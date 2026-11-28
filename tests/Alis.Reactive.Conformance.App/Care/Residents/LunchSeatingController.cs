using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LunchSeatingController : Controller
{
    [HttpGet(CarePages.LunchSeating)]
    public IActionResult OfTheDiningRoom() => View("/Care/Residents/LunchSeating.cshtml", new LunchSeating([TableFour(), TableSeven()], [ResidentsScripts.ServingStationWithAPagedList]));

    private static LunchTable TableFour()
        => new(4, ["risk-choking"], LunchSeatings.FallRiskAtTable4, [new("Ada Hart", "close"), new("Ben Okoro", "standard"), new("Cora Diaz", "close"), new("Dev Patel", "standard")]);

    private static LunchTable TableSeven()
        => new(7, [], LunchSeatings.FallRiskAtTable7, [new("Eli Stone", "standard"), new("Fay Lin", "close"), new("Gus Wright", "standard")]);
}

public sealed record LunchSeating(IReadOnlyList<LunchTable> Tables, IReadOnlyList<string> Scripts) : IScriptedResidentsPage;

public sealed record LunchTable(int Number, IReadOnlyList<string> Flags, AppEvent FallRisk, IReadOnlyList<LunchSeat> Seats);

public sealed record LunchSeat(string Resident, string Supervision);

public static class LunchSeatings
{
    public static readonly AppEvent FallRiskAtTable4 = AppEvent.Declare();

    public static readonly AppEvent FallRiskAtTable7 = AppEvent.Declare();

    public static readonly AppEvent CountThePlates = AppEvent.Declare();
}
