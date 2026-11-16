using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitBookController : Controller
{
    private const string PlainText = "text/plain";
    private const string BookedForm = "yyyy-MM-ddTHH:mm:sszzz";

    [HttpGet(CarePages.VisitBook)]
    public IActionResult OfTheResident() => View("/Care/Record/VisitBook.cshtml", new VisitBook());

    [HttpGet(VisitBookEndpoints.Visits)]
    public IActionResult Visits() => Json(new VisitBookVisits
    {
        First = new VisitBookEntry { Starts = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero) },
        Second = new VisitBookEntry { Starts = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero) },
    });

    [HttpPost(VisitBookEndpoints.Bookings)]
    public IActionResult Book([FromForm] VisitBook book)
    {
        if (book.Starts is not { } starts) return BadRequest();
        return Content(starts.ToString(BookedForm, CultureInfo.InvariantCulture), PlainText);
    }
}

public sealed class VisitBook
{
    public DateTimeOffset? Starts { get; set; }
}

public sealed class VisitBookVisits
{
    public VisitBookEntry First { get; set; } = new();

    public VisitBookEntry Second { get; set; } = new();
}

public sealed class VisitBookEntry
{
    public DateTimeOffset Starts { get; set; }
}

public static class VisitBookEndpoints
{
    public const string Visits = CarePages.VisitBook + "/visits";

    public const string Bookings = CarePages.VisitBook + "/bookings";
}

public static class VisitBookButtons
{
    public static readonly AppEvent ShowTheVisits = AppEvent.Declare();

    public static readonly AppEvent Book = AppEvent.Declare();
}
