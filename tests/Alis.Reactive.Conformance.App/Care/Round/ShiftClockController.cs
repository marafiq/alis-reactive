using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftClockController : Controller
{
    private static DateTime TheShiftsMoment => new(2026, 11, 9, 14, 30, 5, DateTimeKind.Utc);

    [HttpGet(CarePages.ShiftClock)]
    public IActionResult OfTheShift() => View("/Care/Round/ShiftClock.cshtml", new ShiftClock(TheShiftsMoment, TheShiftsMoment.ToLocalTime(), TheShiftsMoment));

    [HttpGet(CarePages.ShiftClockStampedOnTheHomesClock)]
    public IActionResult StampedOnTheHomesClock()
        => View("/Care/Round/ShiftClock.cshtml", new ShiftClock(TheShiftsMoment, TheShiftsMoment.ToLocalTime(), TheShiftsMoment.ToLocalTime()));

    [HttpPost(ShiftClockEndpoints.Stamps)]
    public IActionResult Stamp([FromForm] ShiftStamp stamp) => Json(new StampedShift(AsBound(stamp.At), AsBound(stamp.Seen)));

    private static string AsBound(DateTime bound) => ShiftClock.WallTimeOf(bound) + " " + bound.Kind.ToString();
}

public sealed record ShiftClock(DateTime Now, DateTime Local, DateTime StampsAt)
{
    public static string WallTimeOf(DateTime time) => time.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
}

public sealed class ShiftStamp
{
    public DateTime At { get; set; }

    public DateTime Seen { get; set; }
}

public sealed record StampedShift(string At, string Seen);

public static class ShiftClockButtons
{
    public static readonly AppEvent ShowTheTimes = AppEvent.Declare();

    public static readonly AppEvent Stamp = AppEvent.Declare();
}

public static class ShiftClockEndpoints
{
    public const string Stamps = CarePages.ShiftClock + "/stamps";
}
