using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalSchedulingController : Controller
{
    [HttpGet(CarePages.ArrivalBooking)]
    public IActionResult ArrivalBooking() => View("/Care/AdmissionClinical/ArrivalBooking.cshtml", new ArrivalBooking { ArrivesOn = new DateTime(2026, 7, 13) });

    [HttpGet(CarePages.ArrivalTransport)]
    public IActionResult ArrivalTransport() => View("/Care/AdmissionClinical/ArrivalTransport.cshtml", new ArrivalTransport());

    [HttpGet(CarePages.ArrivalCalendarReview)]
    public IActionResult ArrivalCalendarReview() => View("/Care/AdmissionClinical/ArrivalCalendarReview.cshtml", new ArrivalCalendarReview { Signed = false, Held = false });

    [HttpGet(CarePages.ArrivalCalendarSigned)]
    public IActionResult ArrivalCalendarSigned() => View("/Care/AdmissionClinical/ArrivalCalendarReview.cshtml", new ArrivalCalendarReview { Signed = true, Held = false });

    [HttpGet(CarePages.ArrivalCalendarHeld)]
    public IActionResult ArrivalCalendarHeld() => View("/Care/AdmissionClinical/ArrivalCalendarReview.cshtml", new ArrivalCalendarReview { Signed = false, Held = true });

    [HttpGet(CarePages.ArrivalInterview)]
    public IActionResult ArrivalInterview() => View("/Care/AdmissionClinical/ArrivalInterview.cshtml", new ArrivalInterview());

    [HttpGet(CarePages.ArrivalWindow)]
    public IActionResult ArrivalWindow() => View("/Care/AdmissionClinical/ArrivalWindow.cshtml", new ArrivalWindow());

    [HttpPost(CarePages.FileArrival)]
    public IActionResult FileArrival([FromForm] ArrivalBooking arrival) => Content(arrival.ArrivesOn is DateTime date
        ? "Arrival booked for " + date.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)
        : "Waiting list without an arrival day", "text/plain");

    [HttpPost(CarePages.FileArrivalTransport)]
    public IActionResult FileTransport([FromForm] ArrivalTransport transport) => Content(transport.ArrivesAt is DateTimeOffset date
        ? "Transport booked for " + date.ToString("dd MMMM yyyy HH:mm zzz", CultureInfo.InvariantCulture)
        : "Transport date unavailable", "text/plain");
}

public sealed class ArrivalBooking
{
    public DateTime? ArrivesOn { get; set; }
}

public sealed class ArrivalTransport
{
    public DateTimeOffset? ArrivesAt { get; set; }
}

public sealed class ArrivalCalendarReview
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);

    public bool Signed { get; init; }

    public bool Held { get; init; }
}

public sealed class ArrivalInterview
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}

public sealed class ArrivalWindow
{
    public DateTime? ArrivesOn { get; set; } = new DateTime(2026, 7, 14);
}
