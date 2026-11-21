using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitTimesController : Controller
{
    private static readonly DateTimeOffset SeenAtNoon = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    [HttpGet(CarePages.VisitTimes)]
    public IActionResult OfTheVisits() => View("/Care/Residents/VisitTimes.cshtml", new VisitTimes(VisitTimesEndpoints.Visits));

    [HttpGet(CarePages.VisitTimesWithTheEarlyVisitAtHalfPastThree)]
    public IActionResult WithTheEarlyVisitAtHalfPastThree()
        => View("/Care/Residents/VisitTimes.cshtml", new VisitTimes(VisitTimesEndpoints.VisitsWithTheEarlyVisitAtHalfPastThree));

    [HttpGet(CarePages.VisitTimesWithTheUtcVisitAtEightThirtyOne)]
    public IActionResult WithTheUtcVisitAtEightThirtyOne()
        => View("/Care/Residents/VisitTimes.cshtml", new VisitTimes(VisitTimesEndpoints.VisitsWithTheUtcVisitAtEightThirtyOne));

    [HttpGet(VisitTimesEndpoints.Visits)]
    public IActionResult Visits() => Json(VisitsAt(new DateTime(2026, 3, 8, 2, 30, 0), new DateTime(2026, 11, 9, 8, 30, 0, DateTimeKind.Utc)));

    [HttpGet(VisitTimesEndpoints.VisitsWithTheEarlyVisitAtHalfPastThree)]
    public IActionResult VisitsWithTheEarlyVisitAtHalfPastThree() => Json(VisitsAt(new DateTime(2026, 3, 8, 3, 30, 0), new DateTime(2026, 11, 9, 8, 30, 0, DateTimeKind.Utc)));

    [HttpGet(VisitTimesEndpoints.VisitsWithTheUtcVisitAtEightThirtyOne)]
    public IActionResult VisitsWithTheUtcVisitAtEightThirtyOne() => Json(VisitsAt(new DateTime(2026, 3, 8, 2, 30, 0), new DateTime(2026, 11, 9, 8, 31, 0, DateTimeKind.Utc)));

    private static List<Visit> VisitsAt(DateTime early, DateTime utc)
    {
        return
        [
            new(early, SeenAtNoon),
            new(new DateTime(2026, 3, 8, 3, 15, 0), SeenAtNoon),
            new(utc, SeenAtNoon),
            new(new DateTime(2026, 11, 9, 8, 30, 0, DateTimeKind.Local), new DateTimeOffset(2026, 11, 9, 13, 0, 0, TimeSpan.FromHours(-5))),
            new(new DateTime(2026, 11, 9, 8, 30, 0), SeenAtNoon),
            new(new DateTime(2026, 11, 9, 8, 30, 0).AddTicks(1), SeenAtNoon),
        ];
    }
}

public sealed record VisitTimes(string VisitsAt);

public sealed record Visit(DateTime At, DateTimeOffset Seen);

public static class VisitTimesEndpoints
{
    public const string Visits = CarePages.Area + "residents/visit-times/visits";
    public const string VisitsWithTheEarlyVisitAtHalfPastThree = Visits + "/the-early-visit-at-half-past-three";
    public const string VisitsWithTheUtcVisitAtEightThirtyOne = Visits + "/the-utc-visit-at-eight-thirty-one";
}
