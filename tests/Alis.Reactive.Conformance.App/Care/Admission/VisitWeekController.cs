using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitWeekController : Controller
{
    [HttpGet(CarePages.VisitWeek)]
    public IActionResult ForAResident() => View("/Care/Admission/VisitWeek.cshtml", new VisitWeek());

    [HttpPost(CarePages.VisitWeek)]
    public IActionResult Save([FromForm] VisitWeek week) => View("/Care/Admission/VisitWeekSaved.cshtml", new VisitWeekSaved(DaysBound(week.VisitDays)));

    private static string DaysBound(List<string> days)
        => days.Count == 0 ? "0 days" : string.Create(CultureInfo.InvariantCulture, $"{days.Count} days: {string.Join(", ", days)}");
}

public sealed class VisitWeek
{
    public List<string> VisitDays { get; set; } = [];
}

public sealed record VisitWeekSaved(string DaysBound);
