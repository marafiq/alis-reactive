using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomComfortController : Controller
{
    [HttpGet(CarePages.RoomComfort)]
    public IActionResult OfTheRoom() => View("/Care/RecordContacts/RoomComfort.cshtml", new RoomComfort { RoomTemperature = 72 });
}

public sealed class RoomComfort
{
    public double? RoomTemperature { get; set; }
}

public static class ComfortChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
