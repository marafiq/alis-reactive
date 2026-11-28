using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PartyBellController : Controller
{
    [HttpGet(CarePages.PartyBell)]
    public IActionResult OfTheGate() => View("/Care/Record/PartyBell.cshtml", new PartyBell("Ada Hart"));
}

public sealed record PartyBell(string Resident);

public sealed class PartyArrival
{
    public int PartySize { get; set; }
}

public static class PartyBells
{
    public static readonly AppEvent<PartyArrival> Announced = AppEvent.Declare<PartyArrival>();
}
