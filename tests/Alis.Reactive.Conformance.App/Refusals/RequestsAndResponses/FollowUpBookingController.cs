using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class FollowUpBookingController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.FollowUpBookingByItsDate)]
    public IActionResult ByItsDate() => View("/Refusals/RequestsAndResponses/FollowUpBookingByItsDate.cshtml", new FollowUpBooking { FollowUp = new DateTime(2026, 12, 1) });

    [HttpGet(RefusalPages.FollowUpBookingWithItsNote)]
    public IActionResult WithItsNote() => View("/Refusals/RequestsAndResponses/FollowUpBookingWithItsNote.cshtml", new FollowUpBooking { FollowUp = new DateTime(2026, 12, 1) });

    [HttpGet(RequestsAndResponsesEndpoints.FollowUpBookings)]
    public IActionResult Book([FromQuery] FollowUpBookingQuery query)
    {
        var followUp = query.Visits.LastOrDefault();
        if (followUp is null) return Content("No visits", PlainText);
        return Content($"{query.Visits.Count} visits, the follow-up on {followUp.When.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} for {followUp.Note}", PlainText);
    }
}

public sealed class FollowUpBooking
{
    public DateTime FollowUp { get; set; }
}

public sealed class FollowUpBookingQuery
{
    public List<FollowUpVisit> Visits { get; set; } = [];
}

public sealed class FollowUpVisit
{
    public DateTime When { get; set; }

    public string? Note { get; set; }
}
