using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalsBoardController : Controller
{
    [HttpGet(CarePages.ArrivalsBoard)]
    public IActionResult OfTheRecord() => View("/Care/Record/ArrivalsBoard.cshtml", new ArrivalsBoard("Ada Hart"));
}

public sealed record ArrivalsBoard(string Resident);

public sealed class Arrival
{
    public string? Nurse { get; set; }
}

public static class ArrivalsBoards
{
    public static readonly AppEvent<Arrival> Arrived = AppEvent.Declare<Arrival>();
}
