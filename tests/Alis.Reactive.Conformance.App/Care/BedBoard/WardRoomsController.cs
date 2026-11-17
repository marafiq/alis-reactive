using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardRoomsController : Controller
{
    [HttpGet(CarePages.WardRooms)]
    public IActionResult OfTheWard() => View("/Care/BedBoard/WardRooms.cshtml", OfNorth());

    [HttpGet(CarePages.WardRoomsBedsFirst)]
    public IActionResult OfTheWardBedsFirst() => View("/Care/BedBoard/WardRoomsBedsFirst.cshtml", OfNorth());

    private static WardRooms OfNorth() => new(
        [new AgedResident("Ann", 84), new AgedResident("Bo", 93)],
        [new WardRoom("North 1", 3), new WardRoom("North 2", 1)]);
}

public sealed class WardRooms(IReadOnlyList<AgedResident> residents, IReadOnlyList<WardRoom> rooms)
{
    public IReadOnlyList<AgedResident> Residents { get; } = residents;

    public IReadOnlyList<WardRoom> Rooms { get; } = rooms;

    public string[]? ResidentNames { get; set; }

    public string[]? RoomNames { get; set; }
}

public sealed record AgedResident(string Name, int Age);

public sealed record WardRoom(string Room, int Beds);
