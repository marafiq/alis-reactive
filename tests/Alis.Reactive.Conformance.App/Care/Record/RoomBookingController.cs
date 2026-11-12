using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomBookingController : Controller
{
    [HttpGet(CarePages.RoomBooking)]
    public IActionResult ForAResident() => View("/Care/Record/RoomBooking.cshtml", new RoomBooking("Ada Hart", false));
}

public sealed record RoomBooking(string Resident, bool RoomFree);

public static class RoomBookings
{
    public static readonly AppEvent Book = AppEvent.Declare();
}
