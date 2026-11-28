using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentTimelineController : Controller
{
    private const string Page = "/Care/Residents/ResidentTimeline.cshtml";
    private const string Resident = "Ada Hart";

    [HttpGet(CarePages.ResidentTimeline)]
    public IActionResult EarliestVisitFirst() => View(Page, new ResidentTimeline(Resident, LatestVisitFirst: false));

    [HttpGet(CarePages.ResidentTimelineLatestVisitFirst)]
    public IActionResult LatestVisitFirst() => View(Page, new ResidentTimeline(Resident, LatestVisitFirst: true));

    [HttpGet(ResidentTimelineEndpoints.Entries)]
    public IActionResult Entries() => Json(new TimelineEntries
    {
        Visits =
        [
            new TimelineVisit { Label = "03:15", At = new DateTime(2026, 3, 8, 3, 15, 0) },
            new TimelineVisit { Label = "01:30", At = new DateTime(2026, 3, 8, 1, 30, 0) },
            new TimelineVisit { Label = "02:30", At = new DateTime(2026, 3, 8, 2, 30, 0) },
        ],
        Sightings =
        [
            new TimelineSighting { Label = "18:00:00.0000001+00:00", Seen = new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero).AddTicks(1) },
            new TimelineSighting { Label = "10:00:00-08:00", Seen = new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.FromHours(-8)) },
            new TimelineSighting { Label = "13:00:00-05:00", Seen = new DateTimeOffset(2026, 11, 9, 13, 0, 0, TimeSpan.FromHours(-5)) },
            new TimelineSighting { Label = "12:00:00-08:00", Seen = new DateTimeOffset(2026, 11, 9, 12, 0, 0, TimeSpan.FromHours(-8)) },
        ],
        Stays =
        [
            new TimelineStay { Label = "23:00:00", Length = TimeSpan.FromHours(23) },
            new TimelineStay { Label = "-00:00:00.5000000", Length = TimeSpan.FromMilliseconds(-500) },
            new TimelineStay { Label = "1.02:00:00", Length = new TimeSpan(1, 2, 0, 0) },
        ],
    });
}

public sealed record ResidentTimeline(string Resident, bool LatestVisitFirst);

public sealed class TimelineEntries
{
    public List<TimelineVisit> Visits { get; set; } = [];

    public List<TimelineSighting> Sightings { get; set; } = [];

    public List<TimelineStay> Stays { get; set; } = [];
}

public sealed class TimelineVisit
{
    public string Label { get; set; } = "";

    public DateTime At { get; set; }
}

public sealed class TimelineSighting
{
    public string Label { get; set; } = "";

    public DateTimeOffset Seen { get; set; }
}

public sealed class TimelineStay
{
    public string Label { get; set; } = "";

    public TimeSpan Length { get; set; }
}

public static class ResidentTimelineEndpoints
{
    public const string Entries = CarePages.ResidentTimeline + "/entries";
}
