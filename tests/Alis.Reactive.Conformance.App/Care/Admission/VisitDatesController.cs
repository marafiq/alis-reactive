using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitDatesController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.VisitDates)]
    public IActionResult Dates() => View("/Care/Admission/VisitDates.cshtml", new VisitDates { Dates = [new DateTime(2026, 11, 30), new DateTime(2026, 12, 1)] });

    [HttpPost(VisitDatesEndpoints.Visits)]
    public IActionResult Save([FromForm] VisitDates visits)
        => Content($"{visits.Resident}: visits on {string.Join(", ", visits.Dates.Select(date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))}", PlainText);
}

public sealed class VisitDates
{
    public string? Resident { get; set; }

    public List<DateTime> Dates { get; set; } = [];
}

public static class VisitDatesEndpoints
{
    public const string Visits = CarePages.VisitDates + "/visits";
}
