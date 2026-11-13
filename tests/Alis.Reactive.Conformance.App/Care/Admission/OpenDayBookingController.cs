using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class OpenDayBookingController : Controller
{
    [HttpGet(CarePages.OpenDayBooking)]
    public IActionResult OfAResident() => View("/Care/Admission/OpenDayBooking.cshtml", new OpenDayBooking("Ada Hart"));
}

public sealed record OpenDayBooking(string Resident);

public static class OpenDayBookings
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
