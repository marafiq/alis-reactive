using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomBedsController : Controller
{
    [HttpGet(CarePages.RoomBeds)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/RoomBeds.cshtml", new RoomBeds([new RoomBed("East 1", "East"), new RoomBed("East 2", "East")]));
}

public sealed record RoomBeds(IReadOnlyList<RoomBed> Beds);

public sealed record RoomBed([property: JsonPropertyName("room")] string RoomNumber, string Ward);
