using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class DepartureController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.DepartureRoomTwice)]
    public IActionResult RoomTwice() => View("/Refusals/InputsAndIds/DepartureRoomTwice.cshtml", new DeparturePage());

    [HttpGet(RefusalPages.DepartureRoomOnce)]
    public IActionResult RoomOnce() => View("/Refusals/InputsAndIds/DepartureRoomOnce.cshtml", new DeparturePage());

    [HttpPost(RefusalEndpoints.Departures)]
    public IActionResult Save([FromForm] DeparturePage departure)
        => Content(departure.Room is { } room ? $"Departing from room {room}" : "No room given", PlainText);
}

public sealed class DeparturePage
{
    public string? Room { get; set; }
}

public static class Departures
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
